using LMStore.Domain.Common;
using LMStore.Domain.Exceptions;
using LMStore.Domain.ValueObjects;

namespace LMStore.Domain.Entities;

public class ItemCarrinho : Entity
{
    public Guid CarrinhoId { get; }
    public Guid VariacaoProdutoId { get; }
    public Dinheiro PrecoUnitario { get; private set; }
    public int Quantidade { get; private set; }
    public Dinheiro CalcularSubtotal() => PrecoUnitario.MultiplicarPor(Quantidade);

    internal ItemCarrinho(Guid carrinhoId, Guid variacaoProdutoId, Dinheiro precoUnitario, int quantidade)
    {
        if (quantidade <= 0)
            throw new DomainException("A quantidade deve ser maior que zero.");

        CarrinhoId = carrinhoId;
        VariacaoProdutoId = variacaoProdutoId;
        PrecoUnitario = precoUnitario;
        Quantidade = quantidade;
    }

    internal void AlterarQuantidade(int novaQuantidade)
    {
        if (novaQuantidade <= 0)
            throw new DomainException("A quantidade deve ser maior que zero.");

        Quantidade = novaQuantidade;
    }

    internal void AtualizarPrecoUnitario(Dinheiro precoUnitario)
    {
        PrecoUnitario = precoUnitario;
    }
}
