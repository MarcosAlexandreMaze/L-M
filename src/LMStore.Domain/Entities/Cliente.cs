using LMStore.Domain.Common;
using LMStore.Domain.Enums;
using LMStore.Domain.Exceptions;
using LMStore.Domain.ValueObjects;

namespace LMStore.Domain.Entities;

public class Cliente : Entity
{
    private readonly List<Endereco> _enderecos = [];

    public Guid UsuarioId { get; }
    public string Nome { get; private set; }
    public Cpf Cpf { get; }
    public string Telefone { get; private set; }
    public DateOnly DataNascimento { get; }
    public StatusCliente Status { get; private set; }
    public IReadOnlyList<Endereco> Enderecos => _enderecos.AsReadOnly();

    public Cliente(Guid usuarioId, string nome, Cpf cpf, string telefone, DateOnly dataNascimento)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do cliente é obrigatório.");

        if (string.IsNullOrWhiteSpace(telefone))
            throw new DomainException("O telefone do cliente é obrigatório.");

        if (dataNascimento > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new DomainException("A data de nascimento não pode estar no futuro.");

        UsuarioId = usuarioId;
        Nome = nome.Trim();
        Cpf = cpf;
        Telefone = telefone.Trim();
        DataNascimento = dataNascimento;
        Status = StatusCliente.Ativo;
    }

    public Endereco AdicionarEndereco(
        string apelido, Cep cep, string logradouro, string numero, string? complemento,
        string bairro, string cidade, string estado, string pais = "Brasil")
    {
        var definirComoPadrao = _enderecos.Count == 0;

        var endereco = new Endereco(
            Id, apelido, cep, logradouro, numero, complemento, bairro, cidade, estado, pais, definirComoPadrao);

        _enderecos.Add(endereco);

        return endereco;
    }

    public void RemoverEndereco(Guid enderecoId)
    {
        var endereco = BuscarEnderecoOuFalhar(enderecoId);
        var eraPadrao = endereco.Padrao;

        _enderecos.Remove(endereco);

        if (eraPadrao && _enderecos.Count > 0)
            _enderecos[0].DefinirComoPadrao();
    }

    public void AtualizarEndereco(
        Guid enderecoId, string apelido, Cep cep, string logradouro, string numero,
        string? complemento, string bairro, string cidade, string estado, string pais)
    {
        var endereco = BuscarEnderecoOuFalhar(enderecoId);
        endereco.Atualizar(apelido, cep, logradouro, numero, complemento, bairro, cidade, estado, pais);
    }

    public void DefinirEnderecoPadrao(Guid enderecoId)
    {
        var enderecoPadrao = BuscarEnderecoOuFalhar(enderecoId);

        foreach (var endereco in _enderecos)
            endereco.RemoverComoPadrao();

        enderecoPadrao.DefinirComoPadrao();
    }

    public void AtualizarDadosCadastrais(string nome, string telefone)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do cliente é obrigatório.");

        if (string.IsNullOrWhiteSpace(telefone))
            throw new DomainException("O telefone do cliente é obrigatório.");

        Nome = nome.Trim();
        Telefone = telefone.Trim();
    }

    public void Ativar()
    {
        if (Status == StatusCliente.Ativo)
            throw new DomainException("Este cliente já está ativo.");

        Status = StatusCliente.Ativo;
    }

    public void Desativar()
    {
        if (Status == StatusCliente.Inativo)
            throw new DomainException("Este cliente já está inativo.");

        Status = StatusCliente.Inativo;
    }

    private Endereco BuscarEnderecoOuFalhar(Guid enderecoId) =>
        _enderecos.FirstOrDefault(e => e.Id == enderecoId)
            ?? throw new NotFoundException($"Endereço '{enderecoId}' não encontrado para este cliente.");
}
