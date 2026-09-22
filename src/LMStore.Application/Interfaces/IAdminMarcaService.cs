using LMStore.Application.DTOs.Admin.Marcas;
using LMStore.Application.DTOs.Catalogo;

namespace LMStore.Application.Interfaces;

public interface IAdminMarcaService
{
    Task<MarcaResponse> CriarAsync(CriarMarcaRequest request, CancellationToken ct = default);
    Task<MarcaResponse> RenomearAsync(Guid marcaId, RenomearMarcaRequest request, CancellationToken ct = default);
    Task<MarcaResponse> AtivarAsync(Guid marcaId, CancellationToken ct = default);
    Task<MarcaResponse> DesativarAsync(Guid marcaId, CancellationToken ct = default);
}
