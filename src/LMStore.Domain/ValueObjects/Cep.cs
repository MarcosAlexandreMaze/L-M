using LMStore.Domain.Exceptions;

namespace LMStore.Domain.ValueObjects;

public sealed record Cep
{
    public string Numero { get; }

    public Cep(string numero)
    {
        var digitos = new string([.. (numero ?? string.Empty).Where(char.IsDigit)]);

        if (digitos.Length != 8)
            throw new DomainException($"'{numero}' não é um CEP válido — são esperados 8 dígitos.");

        Numero = digitos;
    }

    public string Formatado => $"{Numero[..5]}-{Numero[5..]}";

    public override string ToString() => Formatado;
}
