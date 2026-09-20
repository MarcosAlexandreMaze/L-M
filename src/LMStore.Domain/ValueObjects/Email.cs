using System.Text.RegularExpressions;
using LMStore.Domain.Exceptions;

namespace LMStore.Domain.ValueObjects;

public sealed partial record Email
{
    public string Endereco { get; }

    public Email(string endereco)
    {
        if (string.IsNullOrWhiteSpace(endereco))
            throw new DomainException("O e-mail não pode ser vazio.");

        endereco = endereco.Trim();

        if (!FormatoValido().IsMatch(endereco))
            throw new DomainException($"'{endereco}' não é um endereço de e-mail válido.");

        Endereco = endereco.ToLowerInvariant();
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex FormatoValido();

    public override string ToString() => Endereco;
}
