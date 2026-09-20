namespace LMStore.Domain.ValueObjects;

public sealed record EnderecoEntrega(
    string Logradouro, string Numero, string? Complemento, string Bairro,
    string Cidade, string Estado, Cep Cep, string Pais)
{
    public override string ToString() =>
        string.IsNullOrEmpty(Complemento)
            ? $"{Logradouro}, {Numero} - {Bairro}, {Cidade}/{Estado} - {Cep}"
            : $"{Logradouro}, {Numero} - {Complemento} - {Bairro}, {Cidade}/{Estado} - {Cep}";
}
