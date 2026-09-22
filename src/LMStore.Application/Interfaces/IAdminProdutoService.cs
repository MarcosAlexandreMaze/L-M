using LMStore.Application.DTOs.Admin.Produtos;
using LMStore.Application.DTOs.Catalogo;

namespace LMStore.Application.Interfaces;

public interface IAdminProdutoService
{
    Task<ProdutoDetalheResponse> CriarRoupaAsync(CriarRoupaRequest request, CancellationToken ct = default);
    Task<ProdutoDetalheResponse> CriarTenisAsync(CriarTenisRequest request, CancellationToken ct = default);
    Task<ProdutoDetalheResponse> AtualizarAsync(Guid produtoId, AtualizarProdutoRequest request, CancellationToken ct = default);
    Task<ProdutoDetalheResponse> AplicarPromocaoAsync(Guid produtoId, AplicarPromocaoRequest request, CancellationToken ct = default);
    Task<ProdutoDetalheResponse> RemoverPromocaoAsync(Guid produtoId, CancellationToken ct = default);
    Task<ProdutoDetalheResponse> AtivarAsync(Guid produtoId, CancellationToken ct = default);
    Task<ProdutoDetalheResponse> DesativarAsync(Guid produtoId, CancellationToken ct = default);
    Task<ProdutoDetalheResponse> AdicionarVariacaoAsync(Guid produtoId, AdicionarVariacaoRequest request, CancellationToken ct = default);
    Task<ProdutoDetalheResponse> AtivarVariacaoAsync(Guid produtoId, Guid variacaoId, CancellationToken ct = default);
    Task<ProdutoDetalheResponse> DesativarVariacaoAsync(Guid produtoId, Guid variacaoId, CancellationToken ct = default);
}
