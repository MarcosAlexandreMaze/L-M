using LMStore.Application.DTOs.Common;
using LMStore.Application.DTOs.Pagamento;
using LMStore.Application.DTOs.Pedido;
using LMStore.Application.Interfaces;
using LMStore.Application.Mappings;
using LMStore.Domain.Entities;
using LMStore.Domain.Exceptions;
using LMStore.Domain.Interfaces;
using LMStore.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace LMStore.Application.Services;

public class PedidoService(
    IPedidoRepository pedidoRepository,
    ICarrinhoRepository carrinhoRepository,
    IClienteRepository clienteRepository,
    IProdutoRepository produtoRepository,
    IEstoqueRepository estoqueRepository,
    ICupomRepository cupomRepository,
    IPaymentGateway paymentGateway,
    IShippingCalculator shippingCalculator,
    IUnitOfWork unitOfWork,
    ILogger<PedidoService> logger) : IPedidoService
{
    public async Task<PedidoResponse> CheckoutAsync(Guid usuarioId, CheckoutRequest request, CancellationToken ct = default)
    {
        var cliente = await clienteRepository.ObterPorUsuarioIdAsync(usuarioId, ct)
            ?? throw new NotFoundException("Cliente não encontrado para este usuário.");

        var clienteComEnderecos = await clienteRepository.ObterPorIdComEnderecosAsync(cliente.Id, ct)
            ?? throw new NotFoundException("Cliente não encontrado.");

        var enderecoEscolhido = clienteComEnderecos.Enderecos.FirstOrDefault(e => e.Id == request.EnderecoEntregaId)
            ?? throw new NotFoundException("Endereço de entrega não encontrado para este cliente.");

        var carrinho = await carrinhoRepository.ObterPorClienteIdAsync(cliente.Id, ct)
            ?? throw new DomainException("Carrinho não encontrado.");

        if (carrinho.EstaVazio())
            throw new DomainException("Não é possível finalizar a compra com o carrinho vazio.");

        var itensParaPedido = await MontarItensParaPedidoAsync(carrinho, ct);
        var cupomAplicado = await ResolverCupomAsync(request.CodigoCupom, carrinho.CupomAplicadoId, ct);

        var enderecoEntrega = new EnderecoEntrega(
            enderecoEscolhido.Logradouro, enderecoEscolhido.Numero, enderecoEscolhido.Complemento,
            enderecoEscolhido.Bairro, enderecoEscolhido.Cidade, enderecoEscolhido.Estado,
            enderecoEscolhido.Cep, enderecoEscolhido.Pais);

        var valorFrete = new Dinheiro(await shippingCalculator.CalcularAsync(enderecoEscolhido.Cep, ct));

        var pedido = Pedido.CriarDeCarrinho(
            cliente.Id, itensParaPedido, enderecoEntrega, valorFrete, request.FormaPagamento, cupomAplicado);

        // Criação do pedido + reserva de estoque numa única transação, itens ordenados
        // por Id de variação — mesma ordem sempre, para dois checkouts concorrentes
        // nunca travarem um no outro esperando a mesma dupla de linhas em ordem
        // trocada (decisão validada na Etapa 7). A reserva em si (ReservarAsync) é um
        // UPDATE condicional atômico — não depende de carregar o Estoque em memória.
        await unitOfWork.ExecutarEmTransacaoAsync(async () =>
        {
            pedidoRepository.Adicionar(pedido);
            await unitOfWork.SalvarAsync(ct);

            foreach (var item in pedido.Itens.OrderBy(i => i.VariacaoProdutoId))
                await estoqueRepository.ReservarAsync(item.VariacaoProdutoId, item.Quantidade, pedido.Id, ct);

            await unitOfWork.SalvarAsync(ct);
        }, ct);

        // O pagamento roda FORA da transação de reserva de propósito: uma chamada a um
        // gateway externo pode ser lenta ou instável, e transações de banco devem ficar
        // curtas. Se o pagamento falhar ou demorar, o estoque já está reservado (não se
        // perde) e o pedido pode ser tentado de novo via PagarNovamenteAsync.
        await ProcessarPagamentoAsync(pedido, request.TokenPagamento, cupomAplicado, ct);

        carrinho.EsvaziarAposCheckout();
        await unitOfWork.SalvarAsync(ct);

        logger.LogInformation(
            "Pedido {PedidoId} ({Numero}) criado para o cliente {ClienteId} — total {Total}, status {Status}",
            pedido.Id, pedido.Numero, cliente.Id, pedido.Total, pedido.Status);

        return pedido.ParaResponse();
    }

    public async Task<PedidoResponse> ObterPorIdAsync(Guid usuarioId, Guid pedidoId, CancellationToken ct = default)
    {
        var clienteId = await ResolverClienteIdAsync(usuarioId, ct);
        var pedido = await ObterPedidoDoClienteOuFalharAsync(pedidoId, clienteId, ct);

        return pedido.ParaResponse();
    }

    public async Task<PagedResult<PedidoResumoResponse>> ListarMeusPedidosAsync(
        Guid usuarioId, int pagina, int tamanhoPagina, CancellationToken ct = default)
    {
        var clienteId = await ResolverClienteIdAsync(usuarioId, ct);
        var (pedidos, total) = await pedidoRepository.ListarPorClienteAsync(clienteId, pagina, tamanhoPagina, ct);

        return new PagedResult<PedidoResumoResponse>([.. pedidos.Select(p => p.ParaResumo())], pagina, tamanhoPagina, total);
    }

    public async Task<PedidoResponse> PagarNovamenteAsync(
        Guid usuarioId, Guid pedidoId, PagarNovamenteRequest request, CancellationToken ct = default)
    {
        var clienteId = await ResolverClienteIdAsync(usuarioId, ct);
        var pedido = await ObterPedidoDoClienteOuFalharAsync(pedidoId, clienteId, ct);

        pedido.RegistrarNovaTentativaDePagamento();

        Cupom? cupomAplicado = pedido.CupomAplicadoId is null
            ? null
            : await cupomRepository.ObterPorIdAsync(pedido.CupomAplicadoId.Value, ct);

        await ProcessarPagamentoAsync(pedido, request.TokenPagamento, cupomAplicado, ct);
        await unitOfWork.SalvarAsync(ct);

        return pedido.ParaResponse();
    }

    public async Task<PedidoResponse> CancelarAsync(
        Guid usuarioId, Guid pedidoId, CancelarPedidoRequest request, CancellationToken ct = default)
    {
        var clienteId = await ResolverClienteIdAsync(usuarioId, ct);
        var pedido = await ObterPedidoDoClienteOuFalharAsync(pedidoId, clienteId, ct);

        pedido.Cancelar(request.Motivo);

        // Todo status cancelável (AguardandoPagamento/PagamentoAprovado/EmPreparacao)
        // ainda tem a reserva de estoque intacta — ConfirmarSaida só acontece no envio
        // (Etapa 5) — então cancelar sempre devolve a reserva inteira.
        await unitOfWork.ExecutarEmTransacaoAsync(async () =>
        {
            foreach (var item in pedido.Itens)
                await estoqueRepository.CancelarReservaAsync(item.VariacaoProdutoId, item.Quantidade, pedido.Id, ct);

            await unitOfWork.SalvarAsync(ct);
        }, ct);

        logger.LogInformation("Pedido {PedidoId} cancelado pelo cliente. Motivo: {Motivo}", pedido.Id, request.Motivo);

        return pedido.ParaResponse();
    }

    private async Task ProcessarPagamentoAsync(Pedido pedido, string tokenPagamento, Cupom? cupomAplicado, CancellationToken ct)
    {
        var resultado = await paymentGateway.ProcessarAsync(
            new SolicitacaoPagamento(pedido.Id, pedido.FormaPagamento, pedido.Total.Valor, tokenPagamento), ct);

        if (resultado.Aprovado)
        {
            pedido.ConfirmarPagamento(resultado.TransacaoExternaId!);
            cupomAplicado?.RegistrarUso();
            logger.LogInformation("Pagamento aprovado para o pedido {PedidoId} — transação {TransacaoId}",
                pedido.Id, resultado.TransacaoExternaId);
        }
        else
        {
            pedido.RecusarPagamento();
            logger.LogWarning("Pagamento recusado para o pedido {PedidoId}. Motivo: {Motivo}",
                pedido.Id, resultado.MotivoRecusa);
        }
    }

    private async Task<List<ItemParaPedido>> MontarItensParaPedidoAsync(Carrinho carrinho, CancellationToken ct)
    {
        var variacaoIds = carrinho.Itens.Select(i => i.VariacaoProdutoId).ToList();
        var produtos = await produtoRepository.ObterPorVariacaoIdsAsync(variacaoIds, ct);

        return [.. carrinho.Itens.Select(item =>
        {
            var produto = produtos.FirstOrDefault(p => p.Variacoes.Any(v => v.Id == item.VariacaoProdutoId))
                ?? throw new DomainException($"O produto de um dos itens do carrinho não está mais disponível.");
            var variacao = produto.Variacoes.First(v => v.Id == item.VariacaoProdutoId);

            return new ItemParaPedido(item.VariacaoProdutoId, produto.Nome, variacao.Sku, item.PrecoUnitario, item.Quantidade);
        })];
    }

    private async Task<Cupom?> ResolverCupomAsync(string? codigoCupomInformado, Guid? cupomAplicadoNoCarrinho, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(codigoCupomInformado))
        {
            return await cupomRepository.ObterPorCodigoAsync(codigoCupomInformado, ct)
                ?? throw new NotFoundException($"Cupom '{codigoCupomInformado}' não encontrado.");
        }

        return cupomAplicadoNoCarrinho is null ? null : await cupomRepository.ObterPorIdAsync(cupomAplicadoNoCarrinho.Value, ct);
    }

    private async Task<Guid> ResolverClienteIdAsync(Guid usuarioId, CancellationToken ct)
    {
        var cliente = await clienteRepository.ObterPorUsuarioIdAsync(usuarioId, ct)
            ?? throw new NotFoundException("Cliente não encontrado para este usuário.");

        return cliente.Id;
    }

    // 404, não 403, quando o pedido é de outro cliente — não revela nem que o Id existe
    // (evita um cliente autenticado usar respostas diferentes pra "sondar" pedidos
    // alheios por tentativa e erro).
    private async Task<Domain.Entities.Pedido> ObterPedidoDoClienteOuFalharAsync(Guid pedidoId, Guid clienteId, CancellationToken ct)
    {
        var pedido = await pedidoRepository.ObterPorIdAsync(pedidoId, ct);

        if (pedido is null || pedido.ClienteId != clienteId)
            throw new NotFoundException($"Pedido '{pedidoId}' não encontrado.");

        return pedido;
    }
}
