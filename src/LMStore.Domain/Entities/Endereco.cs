using LMStore.Domain.Common;
using LMStore.Domain.Exceptions;
using LMStore.Domain.ValueObjects;

namespace LMStore.Domain.Entities;

public class Endereco : Entity
{
    private static readonly HashSet<string> UfsValidas =
    [
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO",
        "MA", "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI",
        "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    ];

    public Guid ClienteId { get; }
    public string Apelido { get; private set; }
    public Cep Cep { get; private set; }
    public string Logradouro { get; private set; }
    public string Numero { get; private set; }
    public string? Complemento { get; private set; }
    public string Bairro { get; private set; }
    public string Cidade { get; private set; }
    public string Estado { get; private set; }
    public string Pais { get; private set; }
    public bool Padrao { get; private set; }

    internal Endereco(
        Guid clienteId, string apelido, Cep cep, string logradouro, string numero, string? complemento,
        string bairro, string cidade, string estado, string pais, bool padrao)
    {
        ClienteId = clienteId;
        Padrao = padrao;
        Cep = cep;
        Apelido = NormalizarApelido(apelido);
        Logradouro = ExigirPreenchido(logradouro, "logradouro");
        Numero = ExigirPreenchido(numero, "número (use 'S/N' quando não houver numeração)");
        Complemento = string.IsNullOrWhiteSpace(complemento) ? null : complemento.Trim();
        Bairro = ExigirPreenchido(bairro, "bairro");
        Cidade = ExigirPreenchido(cidade, "cidade");
        Estado = ValidarUf(estado);
        Pais = string.IsNullOrWhiteSpace(pais) ? "Brasil" : pais.Trim();
    }

    internal void Atualizar(
        string apelido, Cep cep, string logradouro, string numero, string? complemento,
        string bairro, string cidade, string estado, string pais)
    {
        Cep = cep;
        Apelido = NormalizarApelido(apelido);
        Logradouro = ExigirPreenchido(logradouro, "logradouro");
        Numero = ExigirPreenchido(numero, "número (use 'S/N' quando não houver numeração)");
        Complemento = string.IsNullOrWhiteSpace(complemento) ? null : complemento.Trim();
        Bairro = ExigirPreenchido(bairro, "bairro");
        Cidade = ExigirPreenchido(cidade, "cidade");
        Estado = ValidarUf(estado);
        Pais = string.IsNullOrWhiteSpace(pais) ? "Brasil" : pais.Trim();
    }

    internal void DefinirComoPadrao() => Padrao = true;

    internal void RemoverComoPadrao() => Padrao = false;

    private static string NormalizarApelido(string apelido) =>
        string.IsNullOrWhiteSpace(apelido) ? "Endereço" : apelido.Trim();

    private static string ExigirPreenchido(string valor, string nomeDoCampo)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new DomainException($"O campo '{nomeDoCampo}' é obrigatório.");

        return valor.Trim();
    }

    private static string ValidarUf(string estado)
    {
        var ufNormalizada = (estado ?? string.Empty).Trim().ToUpperInvariant();

        if (!UfsValidas.Contains(ufNormalizada))
            throw new DomainException($"'{estado}' não é uma UF brasileira válida.");

        return ufNormalizada;
    }
}
