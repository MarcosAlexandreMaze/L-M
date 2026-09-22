namespace LMStore.Application.DTOs.Admin.Administradores;

public record AdministradorResponse(Guid Id, string Nome, string? Cargo, string Email);
