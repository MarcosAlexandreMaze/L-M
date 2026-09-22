namespace LMStore.Application.DTOs.Admin.Clientes;

public record ClienteAdminResponse(
    Guid Id,
    string Nome,
    string Cpf,
    string Telefone,
    DateOnly DataNascimento,
    string Status,
    int QuantidadeEnderecos,
    DateTime CriadoEm);
