using LMStore.Domain.Exceptions;

namespace LMStore.Domain.ValueObjects;

public sealed record Sku
{
    private const int TamanhoMaximo = 50;

    public string Codigo { get; }

    public Sku(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new DomainException("O SKU não pode ser vazio.");

        codigo = codigo.Trim().ToUpperInvariant();

        if (codigo.Length > TamanhoMaximo)
            throw new DomainException($"O SKU não pode ter mais de {TamanhoMaximo} caracteres.");

        Codigo = codigo;
    }

    public override string ToString() => Codigo;
}
