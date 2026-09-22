namespace LMStore.Application.DTOs.Admin.Administradores;

public record CriarAdministradorRequest(string Nome, string Email, string Senha, string? Cargo);
