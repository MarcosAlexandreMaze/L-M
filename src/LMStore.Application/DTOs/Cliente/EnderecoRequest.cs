namespace LMStore.Application.DTOs.Cliente;

public record EnderecoRequest(
    string Apelido,
    string Cep,
    string Logradouro,
    string Numero,
    string? Complemento,
    string Bairro,
    string Cidade,
    string Estado,
    string Pais = "Brasil");
