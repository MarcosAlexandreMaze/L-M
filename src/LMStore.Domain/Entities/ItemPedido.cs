using LMStore.Domain.Common;
using LMStore.Domain.Exceptions;
using LMStore.Domain.ValueObjects;

namespace LMStore.Domain.Entities;

public class ItemPedido : Entity
{
    public Guid PedidoId { get; }
    public Guid VariacaoProdutoId { get; }
    public string NomeProdutoSnapshot { get; }
    public Sku SkuSnapshot { get; }
    public Dinheiro PrecoUnitarioSnapshot { get; }
    public int Quantidade { get; }
    public Dinheiro Subtotal => PrecoUnitarioSnapshot.MultiplicarPor(Quantidade);

    internal ItemPedido(
        Guid pedidoId, Guid variacaoProdutoId, string nomeProdutoSnapshot, Sku skuSnapshot,
        Dinheiro precoUnitarioSnapshot, int quantidade)
    {
        if (quantidade <= 0)
            throw new DomainException("A quantidade deve ser maior que zero.");

        PedidoId = pedidoId;
        VariacaoProdutoId = variacaoProdutoId;
        NomeProdutoSnapshot = nomeProdutoSnapshot;
        SkuSnapshot = skuSnapshot;
        PrecoUnitarioSnapshot = precoUnitarioSnapshot;
        Quantidade = quantidade;
    }
}
