using LMStore.Application.DTOs.Carrinho;

namespace LMStore.Application.Interfaces;

public interface ICarrinhoService
{
    Task<CarrinhoResponse> ObterAsync(Guid usuarioId, CancellationToken ct = default);
    Task<CarrinhoResponse> AdicionarItemAsync(Guid usuarioId, AdicionarItemRequest request, CancellationToken ct = default);
    Task<CarrinhoResponse> AlterarQuantidadeAsync(Guid usuarioId, Guid itemId, AlterarQuantidadeRequest request, CancellationToken ct = default);
    Task<CarrinhoResponse> RemoverItemAsync(Guid usuarioId, Guid itemId, CancellationToken ct = default);
    Task<CarrinhoResponse> AplicarCupomAsync(Guid usuarioId, AplicarCupomRequest request, CancellationToken ct = default);
    Task<CarrinhoResponse> RemoverCupomAsync(Guid usuarioId, CancellationToken ct = default);
}
