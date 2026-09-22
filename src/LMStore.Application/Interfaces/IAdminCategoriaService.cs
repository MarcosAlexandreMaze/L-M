using LMStore.Application.DTOs.Admin.Categorias;
using LMStore.Application.DTOs.Catalogo;

namespace LMStore.Application.Interfaces;

public interface IAdminCategoriaService
{
    Task<CategoriaResponse> CriarAsync(CriarCategoriaRequest request, CancellationToken ct = default);
    Task<CategoriaResponse> RenomearAsync(Guid categoriaId, RenomearCategoriaRequest request, CancellationToken ct = default);
    Task<CategoriaResponse> MoverAsync(Guid categoriaId, MoverCategoriaRequest request, CancellationToken ct = default);
    Task<CategoriaResponse> AtivarAsync(Guid categoriaId, CancellationToken ct = default);
    Task<CategoriaResponse> DesativarAsync(Guid categoriaId, CancellationToken ct = default);
}
