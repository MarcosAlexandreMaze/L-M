using LMStore.Application.DTOs.Admin.Estoque;

namespace LMStore.Application.Interfaces;

public interface IAdminEstoqueService
{
    Task<EstoqueResponse> ObterPorVariacaoAsync(Guid variacaoProdutoId, CancellationToken ct = default);
    Task<EstoqueResponse> ReporAsync(Guid variacaoProdutoId, ReporEstoqueRequest request, CancellationToken ct = default);
    Task<EstoqueResponse> AjustarAsync(Guid variacaoProdutoId, AjustarEstoqueRequest request, CancellationToken ct = default);
}
