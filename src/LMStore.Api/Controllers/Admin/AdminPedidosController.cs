using LMStore.Application.DTOs.Admin.Pedidos;
using LMStore.Application.DTOs.Common;
using LMStore.Application.DTOs.Pedido;
using LMStore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMStore.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/pedidos")]
[Authorize(Roles = "Administrador")]
public class AdminPedidosController(IAdminPedidoService adminPedidoService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<PedidoAdminResumoResponse>>> ListarTodos(
        [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken ct = default)
    {
        pagina = Math.Max(pagina, 1);
        tamanhoPagina = Math.Clamp(tamanhoPagina, 1, 100);

        var pedidos = await adminPedidoService.ListarTodosAsync(pagina, tamanhoPagina, ct);
        return Ok(pedidos);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PedidoResponse>> ObterPorId(Guid id, CancellationToken ct)
    {
        var pedido = await adminPedidoService.ObterPorIdAsync(id, ct);
        return Ok(pedido);
    }

    [HttpPost("{id:guid}/preparar")]
    public async Task<ActionResult<PedidoResponse>> IniciarPreparacao(Guid id, CancellationToken ct)
    {
        var pedido = await adminPedidoService.IniciarPreparacaoAsync(id, ct);
        return Ok(pedido);
    }

    [HttpPost("{id:guid}/enviar")]
    public async Task<ActionResult<PedidoResponse>> MarcarComoEnviado(
        Guid id, MarcarComoEnviadoRequest request, CancellationToken ct)
    {
        var pedido = await adminPedidoService.MarcarComoEnviadoAsync(id, request, ct);
        return Ok(pedido);
    }

    [HttpPost("{id:guid}/entregar")]
    public async Task<ActionResult<PedidoResponse>> MarcarComoEntregue(Guid id, CancellationToken ct)
    {
        var pedido = await adminPedidoService.MarcarComoEntregueAsync(id, ct);
        return Ok(pedido);
    }

    [HttpPost("{id:guid}/cancelar")]
    public async Task<ActionResult<PedidoResponse>> Cancelar(Guid id, CancelarPedidoRequest request, CancellationToken ct)
    {
        var pedido = await adminPedidoService.CancelarAsync(id, request, ct);
        return Ok(pedido);
    }
}
