using LMStore.Application.DTOs.Common;
using LMStore.Application.DTOs.Pedido;

namespace LMStore.Application.Interfaces;

public interface IPedidoService
{
    Task<PedidoResponse> CheckoutAsync(Guid usuarioId, CheckoutRequest request, CancellationToken ct = default);
    Task<PedidoResponse> ObterPorIdAsync(Guid usuarioId, Guid pedidoId, CancellationToken ct = default);

    Task<PagedResult<PedidoResumoResponse>> ListarMeusPedidosAsync(
        Guid usuarioId, int pagina, int tamanhoPagina, CancellationToken ct = default);

    Task<PedidoResponse> PagarNovamenteAsync(
        Guid usuarioId, Guid pedidoId, PagarNovamenteRequest request, CancellationToken ct = default);

    Task<PedidoResponse> CancelarAsync(
        Guid usuarioId, Guid pedidoId, CancelarPedidoRequest request, CancellationToken ct = default);
}
