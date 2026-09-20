using LMStore.Domain.Common;
using LMStore.Domain.Enums;
using LMStore.Domain.Exceptions;
using LMStore.Domain.ValueObjects;

namespace LMStore.Domain.Entities;

public class Pedido : Entity
{
    private readonly List<ItemPedido> _itens = [];
    private readonly List<Pagamento> _pagamentos = [];

    public Guid ClienteId { get; }
    public string Numero { get; private set; }
    public EnderecoEntrega EnderecoEntrega { get; private set; } = null!;
    public StatusPedido Status { get; private set; }
    public Dinheiro Subtotal { get; private set; }
    public Dinheiro DescontoCupom { get; private set; }
    public Dinheiro ValorFrete { get; }
    public Dinheiro Total { get; private set; }
    public Guid? CupomAplicadoId { get; }
    public FormaPagamento FormaPagamento { get; }
    public string? MotivoCancelamento { get; private set; }
    public IReadOnlyList<ItemPedido> Itens => _itens.AsReadOnly();
    public IReadOnlyList<Pagamento> Pagamentos => _pagamentos.AsReadOnly();
    public Pagamento? PagamentoAtual => _pagamentos.LastOrDefault();

    private Pedido(Guid clienteId, Dinheiro valorFrete, FormaPagamento formaPagamento, Guid? cupomAplicadoId)
    {
        ClienteId = clienteId;
        ValorFrete = valorFrete;
        FormaPagamento = formaPagamento;
        CupomAplicadoId = cupomAplicadoId;
        Status = StatusPedido.AguardandoPagamento;
        Subtotal = Dinheiro.Zero;
        DescontoCupom = Dinheiro.Zero;
        Total = Dinheiro.Zero;
        // Guid.CreateVersion7() é ordenável por tempo: os PRIMEIROS bytes são o timestamp,
        // então dois pedidos no mesmo milissegundo teriam prefixo idêntico. Os ÚLTIMOS
        // caracteres do formato "N" caem na porção realmente aleatória (rand_b, RFC 9562).
        Numero = $"LM{CriadoEm:yyyyMMdd}-{Id.ToString("N")[^8..].ToUpperInvariant()}";
    }

    public static Pedido CriarDeCarrinho(
        Guid clienteId, IReadOnlyList<ItemParaPedido> itens, EnderecoEntrega enderecoEntrega,
        Dinheiro valorFrete, FormaPagamento formaPagamento, Cupom? cupomAplicado = null)
    {
        if (itens.Count == 0)
            throw new DomainException("Não é possível criar um pedido sem itens.");

        // EnderecoEntrega é atribuído aqui fora, não pelo construtor: tipos possuídos
        // (OwnsOne) não podem ser vinculados via parâmetro de construtor no EF Core, então
        // a propriedade precisa de um setter (privado, só acessível dentro desta classe).
        var pedido = new Pedido(clienteId, valorFrete, formaPagamento, cupomAplicado?.Id)
        {
            EnderecoEntrega = enderecoEntrega
        };

        foreach (var item in itens)
        {
            pedido._itens.Add(new ItemPedido(
                pedido.Id, item.VariacaoProdutoId, item.NomeProduto, item.Sku, item.PrecoUnitario, item.Quantidade));
        }

        pedido.CalcularTotais(cupomAplicado);
        pedido._pagamentos.Add(new Pagamento(pedido.Id, formaPagamento, pedido.Total));

        return pedido;
    }

    public void ConfirmarPagamento(string tokenTransacaoExterna)
    {
        ExigirStatusAtual(StatusPedido.AguardandoPagamento, "confirmar o pagamento");

        // Nunca nulo aqui: CriarDeCarrinho sempre inicia o pedido com um pagamento pendente.
        PagamentoAtual!.Aprovar(tokenTransacaoExterna);
        Status = StatusPedido.PagamentoAprovado;
    }

    public void RecusarPagamento()
    {
        ExigirStatusAtual(StatusPedido.AguardandoPagamento, "recusar o pagamento");

        PagamentoAtual!.Recusar();
        // Status permanece AguardandoPagamento — o cliente pode tentar outro meio de pagamento.
    }

    public void RegistrarNovaTentativaDePagamento()
    {
        ExigirStatusAtual(StatusPedido.AguardandoPagamento, "registrar uma nova tentativa de pagamento");

        if (PagamentoAtual?.Status == StatusPagamento.Pendente)
            throw new DomainException("Já existe uma tentativa de pagamento pendente para este pedido.");

        _pagamentos.Add(new Pagamento(Id, FormaPagamento, Total));
    }

    public void IniciarPreparacao()
    {
        ExigirStatusAtual(StatusPedido.PagamentoAprovado, "iniciar a preparação");

        Status = StatusPedido.EmPreparacao;
    }

    public void MarcarComoEnviado(string codigoRastreio)
    {
        ExigirStatusAtual(StatusPedido.EmPreparacao, "marcar como enviado");

        if (string.IsNullOrWhiteSpace(codigoRastreio))
            throw new DomainException("O código de rastreio é obrigatório para marcar o pedido como enviado.");

        Status = StatusPedido.Enviado;
    }

    public void MarcarComoEntregue()
    {
        ExigirStatusAtual(StatusPedido.Enviado, "marcar como entregue");

        Status = StatusPedido.Entregue;
    }

    public void Cancelar(string motivo)
    {
        if (Status is not (StatusPedido.AguardandoPagamento or StatusPedido.PagamentoAprovado or StatusPedido.EmPreparacao))
            throw new DomainException(
                $"Pedidos no status '{Status}' não podem ser cancelados — pedidos enviados ou entregues " +
                "seguem o fluxo de devolução, não o de cancelamento.");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new DomainException("É necessário informar o motivo do cancelamento.");

        Status = StatusPedido.Cancelado;
        MotivoCancelamento = motivo.Trim();
    }

    private void CalcularTotais(Cupom? cupomAplicado)
    {
        Subtotal = _itens.Aggregate(Dinheiro.Zero, (total, item) => total.Somar(item.Subtotal));
        DescontoCupom = cupomAplicado?.CalcularDesconto(Subtotal) ?? Dinheiro.Zero;
        Total = Subtotal.Subtrair(DescontoCupom).Somar(ValorFrete);
    }

    private void ExigirStatusAtual(StatusPedido statusEsperado, string acao)
    {
        if (Status != statusEsperado)
            throw new DomainException($"Não é possível {acao}: o pedido está no status '{Status}'.");
    }
}
