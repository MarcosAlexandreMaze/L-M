using LMStore.Application.DTOs.Catalogo;
using LMStore.Application.DTOs.Common;
using LMStore.Domain.Enums;

namespace LMStore.Application.Interfaces;

public interface ICatalogoService
{
    Task<PagedResult<ProdutoResumoResponse>> BuscarProdutosAsync(
        string? nome, Guid? categoriaId, Guid? marcaId, GeneroProduto? genero,
        decimal? precoMinimo, decimal? precoMaximo, int pagina, int tamanhoPagina, CancellationToken ct = default);

    Task<ProdutoDetalheResponse> ObterProdutoPorIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<CategoriaResponse>> ListarCategoriasAsync(CancellationToken ct = default);

    Task<IReadOnlyList<MarcaResponse>> ListarMarcasAsync(CancellationToken ct = default);
}
