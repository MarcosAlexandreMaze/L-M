using LMStore.Application.DTOs.Admin.Cupons;

namespace LMStore.Application.Interfaces;

public interface IAdminCupomService
{
    Task<CupomResponse> CriarAsync(CriarCupomRequest request, CancellationToken ct = default);
    Task<CupomResponse> AtivarAsync(Guid cupomId, CancellationToken ct = default);
    Task<CupomResponse> DesativarAsync(Guid cupomId, CancellationToken ct = default);
}
