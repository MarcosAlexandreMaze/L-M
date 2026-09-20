using LMStore.Domain.Enums;
using LMStore.Domain.Exceptions;
using LMStore.Domain.ValueObjects;

namespace LMStore.Domain.Entities;

public sealed class Roupa : Produto
{
    public TipoRoupa TipoRoupa { get; }
    public string Material { get; private set; }

    public Roupa(
        string nome, string descricao, Guid marcaId, Guid categoriaId, Dinheiro preco,
        GeneroProduto genero, TipoRoupa tipoRoupa, string material)
        : base(nome, descricao, marcaId, categoriaId, preco, genero)
    {
        if (string.IsNullOrWhiteSpace(material))
            throw new DomainException("O material da roupa é obrigatório.");

        TipoRoupa = tipoRoupa;
        Material = material.Trim();
    }

    public void AtualizarMaterial(string material)
    {
        if (string.IsNullOrWhiteSpace(material))
            throw new DomainException("O material da roupa é obrigatório.");

        Material = material.Trim();
    }

    public override string DetalhesEspecificos() => $"{TipoRoupa} · Material: {Material}";
}
