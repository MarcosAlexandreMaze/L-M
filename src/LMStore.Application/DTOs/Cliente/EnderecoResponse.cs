namespace LMStore.Application.DTOs.Cliente;

public record EnderecoResponse(
    Guid Id,
    string Apelido,
    string Cep,
    string Logradouro,
    string Numero,
    string? Complemento,
    string Bairro,
    string Cidade,
    string Estado,
    string Pais,
    bool Padrao);
