using LMStore.Domain.Common;
using LMStore.Domain.Exceptions;
using LMStore.Domain.ValueObjects;

namespace LMStore.Domain.Entities;

public class VariacaoProduto : Entity
{
    public Guid ProdutoId { get; }
    public Sku Sku { get; }
    public string? CodigoBarras { get; }
    public string Tamanho { get; }
    public string Cor { get; }
    public Dinheiro? PrecoAdicional { get; }
    public bool Ativa { get; private set; }

    internal VariacaoProduto(
        Guid produtoId, Sku sku, string? codigoBarras, string tamanho, string cor, Dinheiro? precoAdicional)
    {
        if (string.IsNullOrWhiteSpace(tamanho))
            throw new DomainException("O tamanho da variação é obrigatório.");

        if (string.IsNullOrWhiteSpace(cor))
            throw new DomainException("A cor da variação é obrigatória.");

        ProdutoId = produtoId;
        Sku = sku;
        CodigoBarras = string.IsNullOrWhiteSpace(codigoBarras) ? null : codigoBarras.Trim();
        Tamanho = tamanho.Trim();
        Cor = cor.Trim();
        PrecoAdicional = precoAdicional;
        Ativa = true;
    }

    internal void Ativar()
    {
        if (Ativa)
            throw new DomainException("Esta variação já está ativa.");

        Ativa = true;
    }

    internal void Desativar()
    {
        if (!Ativa)
            throw new DomainException("Esta variação já está inativa.");

        Ativa = false;
    }
}
