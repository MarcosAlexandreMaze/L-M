using LMStore.Application.DTOs.Carrinho;
using LMStore.Application.Interfaces;
using LMStore.Domain.Entities;
using LMStore.Domain.Exceptions;
using LMStore.Domain.Interfaces;
using LMStore.Domain.ValueObjects;

namespace LMStore.Application.Services;

public class CarrinhoService(
    ICarrinhoRepository carrinhoRepository,
    IClienteRepository clienteRepository,
    IProdutoRepository produtoRepository,
    IEstoqueRepository estoqueRepository,
    ICupomRepository cupomRepository,
    IUnitOfWork unitOfWork) : ICarrinhoService
{
    public async Task<CarrinhoResponse> ObterAsync(Guid usuarioId, CancellationToken ct = default)
    {
        var clienteId = await ResolverClienteIdAsync(usuarioId, ct);
        var carrinho = await ObterOuCriarCarrinhoAsync(clienteId, ct);

        // ObterOuCriarCarrinhoAsync só marca o carrinho novo para rastreamento — sem
        // salvar aqui, um GET repetido sem nenhum item adicionado criaria um Carrinho
        // com Id diferente a cada chamada (nunca persistido, descartado no fim da
        // requisição). Salvar aqui garante um Id estável desde a primeira visita.
        await unitOfWork.SalvarAsync(ct);

        return await MapearParaResponseAsync(carrinho, ct);
    }

    public async Task<CarrinhoResponse> AdicionarItemAsync(
        Guid usuarioId, AdicionarItemRequest request, CancellationToken ct = default)
    {
        var clienteId = await ResolverClienteIdAsync(usuarioId, ct);
        var carrinho = await ObterOuCriarCarrinhoAsync(clienteId, ct);

        var produto = (await produtoRepository.ObterPorVariacaoIdsAsync([request.VariacaoProdutoId], ct))
            .FirstOrDefault()
            ?? throw new NotFoundException($"Variação '{request.VariacaoProdutoId}' não encontrada.");

        var variacao = produto.Variacoes.First(v => v.Id == request.VariacaoProdutoId);

        if (!variacao.Ativa)
            throw new DomainException("Esta variação não está disponível para compra.");

        await ValidarEstoqueDisponivelAsync(carrinho, request.VariacaoProdutoId, request.Quantidade, ct);

        var precoUnitario = variacao.PrecoAdicional is null
            ? produto.PrecoVigente()
            : produto.PrecoVigente().Somar(variacao.PrecoAdicional);

        carrinho.AdicionarItem(request.VariacaoProdutoId, precoUnitario, request.Quantidade);
        await unitOfWork.SalvarAsync(ct);

        return await MapearParaResponseAsync(carrinho, ct);
    }

    public async Task<CarrinhoResponse> AlterarQuantidadeAsync(
        Guid usuarioId, Guid itemId, AlterarQuantidadeRequest request, CancellationToken ct = default)
    {
        var clienteId = await ResolverClienteIdAsync(usuarioId, ct);
        var carrinho = await carrinhoRepository.ObterPorClienteIdAsync(clienteId, ct)
            ?? throw new NotFoundException("Carrinho não encontrado.");

        var item = carrinho.Itens.FirstOrDefault(i => i.Id == itemId)
            ?? throw new NotFoundException($"Item '{itemId}' não encontrado no carrinho.");

        var estoque = await estoqueRepository.ObterPorVariacaoAsync(item.VariacaoProdutoId, ct)
            ?? throw new NotFoundException("Estoque não encontrado para esta variação.");

        if (estoque.QuantidadeDisponivel < request.Quantidade)
            throw new DomainException(
                $"Estoque insuficiente: disponível {estoque.QuantidadeDisponivel}, solicitado {request.Quantidade}.");

        carrinho.AlterarQuantidade(itemId, request.Quantidade);
        await unitOfWork.SalvarAsync(ct);

        return await MapearParaResponseAsync(carrinho, ct);
    }

    public async Task<CarrinhoResponse> RemoverItemAsync(Guid usuarioId, Guid itemId, CancellationToken ct = default)
    {
        var clienteId = await ResolverClienteIdAsync(usuarioId, ct);
        var carrinho = await carrinhoRepository.ObterPorClienteIdAsync(clienteId, ct)
            ?? throw new NotFoundException("Carrinho não encontrado.");

        carrinho.RemoverItem(itemId);
        await unitOfWork.SalvarAsync(ct);

        return await MapearParaResponseAsync(carrinho, ct);
    }

    public async Task<CarrinhoResponse> AplicarCupomAsync(
        Guid usuarioId, AplicarCupomRequest request, CancellationToken ct = default)
    {
        var clienteId = await ResolverClienteIdAsync(usuarioId, ct);
        var carrinho = await carrinhoRepository.ObterPorClienteIdAsync(clienteId, ct)
            ?? throw new NotFoundException("Carrinho não encontrado.");

        if (carrinho.EstaVazio())
            throw new DomainException("Não é possível aplicar cupom a um carrinho vazio.");

        var cupom = await cupomRepository.ObterPorCodigoAsync(request.Codigo, ct)
            ?? throw new NotFoundException($"Cupom '{request.Codigo}' não encontrado.");

        var (valido, motivo) = cupom.EstaValidoPara(carrinho.CalcularSubtotal(), DateTime.UtcNow);
        if (!valido)
            throw new DomainException(motivo!);

        carrinho.AplicarCupom(cupom.Id);
        await unitOfWork.SalvarAsync(ct);

        return await MapearParaResponseAsync(carrinho, ct);
    }

    public async Task<CarrinhoResponse> RemoverCupomAsync(Guid usuarioId, CancellationToken ct = default)
    {
        var clienteId = await ResolverClienteIdAsync(usuarioId, ct);
        var carrinho = await carrinhoRepository.ObterPorClienteIdAsync(clienteId, ct)
            ?? throw new NotFoundException("Carrinho não encontrado.");

        carrinho.RemoverCupom();
        await unitOfWork.SalvarAsync(ct);

        return await MapearParaResponseAsync(carrinho, ct);
    }

    private async Task<Guid> ResolverClienteIdAsync(Guid usuarioId, CancellationToken ct)
    {
        var cliente = await clienteRepository.ObterPorUsuarioIdAsync(usuarioId, ct)
            ?? throw new NotFoundException("Cliente não encontrado para este usuário.");

        return cliente.Id;
    }

    private async Task<Carrinho> ObterOuCriarCarrinhoAsync(Guid clienteId, CancellationToken ct)
    {
        var carrinho = await carrinhoRepository.ObterPorClienteIdAsync(clienteId, ct);

        if (carrinho is not null)
            return carrinho;

        carrinho = new Carrinho(clienteId);
        carrinhoRepository.Adicionar(carrinho);

        return carrinho;
    }

    // Checagem "leve": soma o que já está no carrinho com o que está sendo adicionado
    // agora e compara com o disponível. É só uma conveniência de UX — a garantia real,
    // sob concorrência, é a reserva atômica no checkout (Etapa 7). Duas pessoas podem
    // passar por aqui "ao mesmo tempo" achando que há estoque; quem chegar depois no
    // checkout é que vai esbarrar na reserva de verdade.
    private async Task ValidarEstoqueDisponivelAsync(
        Carrinho carrinho, Guid variacaoProdutoId, int quantidadeAdicional, CancellationToken ct)
    {
        var estoque = await estoqueRepository.ObterPorVariacaoAsync(variacaoProdutoId, ct)
            ?? throw new NotFoundException("Estoque não encontrado para esta variação.");

        var quantidadeJaNoCarrinho = carrinho.Itens.FirstOrDefault(i => i.VariacaoProdutoId == variacaoProdutoId)?.Quantidade ?? 0;
        var quantidadeTotal = quantidadeJaNoCarrinho + quantidadeAdicional;

        if (estoque.QuantidadeDisponivel < quantidadeTotal)
            throw new DomainException(
                $"Estoque insuficiente: disponível {estoque.QuantidadeDisponivel}, quantidade total solicitada {quantidadeTotal}.");
    }

    private async Task<CarrinhoResponse> MapearParaResponseAsync(Carrinho carrinho, CancellationToken ct)
    {
        var variacaoIds = carrinho.Itens.Select(i => i.VariacaoProdutoId).Distinct().ToList();
        var produtos = await produtoRepository.ObterPorVariacaoIdsAsync(variacaoIds, ct);

        var itens = carrinho.Itens.Select(item =>
        {
            var produto = produtos.FirstOrDefault(p => p.Variacoes.Any(v => v.Id == item.VariacaoProdutoId));
            var variacao = produto?.Variacoes.FirstOrDefault(v => v.Id == item.VariacaoProdutoId);

            return new ItemCarrinhoResponse(
                item.Id,
                item.VariacaoProdutoId,
                produto?.Nome ?? "Produto indisponível",
                variacao?.Sku.Codigo ?? "-",
                variacao?.Tamanho ?? "-",
                variacao?.Cor ?? "-",
                item.PrecoUnitario.Valor,
                item.Quantidade,
                item.CalcularSubtotal().Valor);
        }).ToList();

        var subtotal = carrinho.CalcularSubtotal();
        string? codigoCupom = null;
        var desconto = Dinheiro.Zero;

        if (carrinho.CupomAplicadoId is not null)
        {
            var cupom = await cupomRepository.ObterPorIdAsync(carrinho.CupomAplicadoId.Value, ct);

            if (cupom is not null)
            {
                codigoCupom = cupom.Codigo;
                var (valido, _) = cupom.EstaValidoPara(subtotal, DateTime.UtcNow);
                if (valido)
                    desconto = cupom.CalcularDesconto(subtotal);
            }
        }

        return new CarrinhoResponse(
            carrinho.Id,
            itens,
            subtotal.Valor,
            codigoCupom,
            desconto.Valor > 0 ? desconto.Valor : null,
            subtotal.Subtrair(desconto).Valor);
    }
}
