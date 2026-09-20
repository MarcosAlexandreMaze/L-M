using LMStore.Domain.Common;
using LMStore.Domain.Exceptions;
using LMStore.Domain.ValueObjects;

namespace LMStore.Domain.Entities;

public class Carrinho : Entity
{
    private readonly List<ItemCarrinho> _itens = [];

    public Guid ClienteId { get; }
    public Guid? CupomAplicadoId { get; private set; }
    public IReadOnlyList<ItemCarrinho> Itens => _itens.AsReadOnly();

    public Carrinho(Guid clienteId)
    {
        ClienteId = clienteId;
    }

    public ItemCarrinho AdicionarItem(Guid variacaoProdutoId, Dinheiro precoUnitario, int quantidade)
    {
        if (quantidade <= 0)
            throw new DomainException("A quantidade deve ser maior que zero.");

        var itemExistente = _itens.FirstOrDefault(i => i.VariacaoProdutoId == variacaoProdutoId);

        if (itemExistente is not null)
        {
            itemExistente.AlterarQuantidade(itemExistente.Quantidade + quantidade);
            return itemExistente;
        }

        var item = new ItemCarrinho(Id, variacaoProdutoId, precoUnitario, quantidade);
        _itens.Add(item);

        return item;
    }

    public void AlterarQuantidade(Guid itemId, int novaQuantidade)
    {
        if (novaQuantidade <= 0)
            throw new DomainException("A quantidade deve ser maior que zero. Para remover o item, utilize RemoverItem.");

        BuscarItemOuFalhar(itemId).AlterarQuantidade(novaQuantidade);
    }

    public void RemoverItem(Guid itemId)
    {
        var item = BuscarItemOuFalhar(itemId);
        _itens.Remove(item);
    }

    public void AtualizarPrecoDoItem(Guid itemId, Dinheiro precoAtual)
    {
        BuscarItemOuFalhar(itemId).AtualizarPrecoUnitario(precoAtual);
    }

    public void AplicarCupom(Guid cupomId) => CupomAplicadoId = cupomId;

    public void RemoverCupom() => CupomAplicadoId = null;

    public bool EstaVazio() => _itens.Count == 0;

    public Dinheiro CalcularSubtotal() =>
        _itens.Aggregate(Dinheiro.Zero, (total, item) => total.Somar(item.CalcularSubtotal()));

    public void EsvaziarAposCheckout()
    {
        _itens.Clear();
        CupomAplicadoId = null;
    }

    private ItemCarrinho BuscarItemOuFalhar(Guid itemId) =>
        _itens.FirstOrDefault(i => i.Id == itemId)
            ?? throw new NotFoundException($"Item '{itemId}' não encontrado neste carrinho.");
}
