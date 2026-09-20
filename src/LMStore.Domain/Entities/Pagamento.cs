using LMStore.Domain.Common;
using LMStore.Domain.Enums;
using LMStore.Domain.Exceptions;
using LMStore.Domain.ValueObjects;

namespace LMStore.Domain.Entities;

public class Pagamento : Entity
{
    public Guid PedidoId { get; }
    public FormaPagamento FormaPagamento { get; }
    public StatusPagamento Status { get; private set; }
    public Dinheiro ValorPago { get; }
    public DateTime? DataProcessamento { get; private set; }
    public string? TokenTransacaoExterna { get; private set; }

    internal Pagamento(Guid pedidoId, FormaPagamento formaPagamento, Dinheiro valorPago)
    {
        PedidoId = pedidoId;
        FormaPagamento = formaPagamento;
        ValorPago = valorPago;
        Status = StatusPagamento.Pendente;
    }

    internal void Aprovar(string tokenTransacaoExterna)
    {
        if (Status != StatusPagamento.Pendente)
            throw new DomainException($"Não é possível aprovar um pagamento no status '{Status}'.");

        if (string.IsNullOrWhiteSpace(tokenTransacaoExterna))
            throw new DomainException("O token da transação externa é obrigatório para aprovar o pagamento.");

        Status = StatusPagamento.Aprovado;
        TokenTransacaoExterna = tokenTransacaoExterna.Trim();
        DataProcessamento = DateTime.UtcNow;
    }

    internal void Recusar()
    {
        if (Status != StatusPagamento.Pendente)
            throw new DomainException($"Não é possível recusar um pagamento no status '{Status}'.");

        Status = StatusPagamento.Recusado;
        DataProcessamento = DateTime.UtcNow;
    }

    internal void Estornar()
    {
        if (Status != StatusPagamento.Aprovado)
            throw new DomainException("Só é possível estornar um pagamento aprovado.");

        Status = StatusPagamento.Estornado;
    }
}
