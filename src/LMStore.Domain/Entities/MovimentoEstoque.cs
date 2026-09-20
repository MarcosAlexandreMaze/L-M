using LMStore.Domain.Common;
using LMStore.Domain.Enums;
using LMStore.Domain.Exceptions;

namespace LMStore.Domain.Entities;

public class MovimentoEstoque : Entity
{
    public Guid EstoqueId { get; }
    public TipoMovimentoEstoque Tipo { get; }
    public int Quantidade { get; }
    public string Motivo { get; }
    public Guid? PedidoId { get; }

    internal MovimentoEstoque(Guid estoqueId, TipoMovimentoEstoque tipo, int quantidade, string motivo, Guid? pedidoId)
    {
        if (quantidade == 0)
            throw new DomainException("A quantidade do movimento não pode ser zero.");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new DomainException("Todo movimento de estoque precisa de um motivo.");

        EstoqueId = estoqueId;
        Tipo = tipo;
        Quantidade = quantidade;
        Motivo = motivo.Trim();
        PedidoId = pedidoId;
    }
}
