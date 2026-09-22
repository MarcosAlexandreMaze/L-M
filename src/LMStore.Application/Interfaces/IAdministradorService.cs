using LMStore.Application.DTOs.Admin.Administradores;

namespace LMStore.Application.Interfaces;

public interface IAdministradorService
{
    Task<AdministradorResponse> CriarAsync(CriarAdministradorRequest request, CancellationToken ct = default);
}
