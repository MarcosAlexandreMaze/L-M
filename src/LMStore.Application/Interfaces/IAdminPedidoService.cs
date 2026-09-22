using LMStore.Application.DTOs.Admin.Pedidos;
using LMStore.Application.DTOs.Common;
using LMStore.Application.DTOs.Pedido;

namespace LMStore.Application.Interfaces;

public interface IAdminPedidoService
{
    Task<PagedResult<PedidoAdminResumoResponse>> ListarTodosAsync(int pagina, int tamanhoPagina, CancellationToken ct = default);
    Task<PedidoResponse> ObterPorIdAsync(Guid pedidoId, CancellationToken ct = default);
    Task<PedidoResponse> IniciarPreparacaoAsync(Guid pedidoId, CancellationToken ct = default);
    Task<PedidoResponse> MarcarComoEnviadoAsync(Guid pedidoId, MarcarComoEnviadoRequest request, CancellationToken ct = default);
    Task<PedidoResponse> MarcarComoEntregueAsync(Guid pedidoId, CancellationToken ct = default);
    Task<PedidoResponse> CancelarAsync(Guid pedidoId, CancelarPedidoRequest request, CancellationToken ct = default);
}
