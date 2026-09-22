using LMStore.Application.DTOs.Common;
using LMStore.Application.DTOs.Pedido;
using LMStore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMStore.Api.Controllers;

[ApiController]
[Route("api/pedidos")]
[Authorize(Roles = "Cliente")]
public class PedidosController(IPedidoService pedidoService, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PedidoResponse>> Checkout(CheckoutRequest request, CancellationToken ct)
    {
        var pedido = await pedidoService.CheckoutAsync(currentUserService.ObterUsuarioId(), request, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = pedido.Id }, pedido);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<PedidoResumoResponse>>> ListarMeusPedidos(
        [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken ct = default)
    {
        pagina = Math.Max(pagina, 1);
        tamanhoPagina = Math.Clamp(tamanhoPagina, 1, 100);

        var pedidos = await pedidoService.ListarMeusPedidosAsync(currentUserService.ObterUsuarioId(), pagina, tamanhoPagina, ct);
        return Ok(pedidos);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PedidoResponse>> ObterPorId(Guid id, CancellationToken ct)
    {
        var pedido = await pedidoService.ObterPorIdAsync(currentUserService.ObterUsuarioId(), id, ct);
        return Ok(pedido);
    }

    [HttpPost("{id:guid}/pagamentos")]
    public async Task<ActionResult<PedidoResponse>> PagarNovamente(Guid id, PagarNovamenteRequest request, CancellationToken ct)
    {
        var pedido = await pedidoService.PagarNovamenteAsync(currentUserService.ObterUsuarioId(), id, request, ct);
        return Ok(pedido);
    }

    [HttpPost("{id:guid}/cancelar")]
    public async Task<ActionResult<PedidoResponse>> Cancelar(Guid id, CancelarPedidoRequest request, CancellationToken ct)
    {
        var pedido = await pedidoService.CancelarAsync(currentUserService.ObterUsuarioId(), id, request, ct);
        return Ok(pedido);
    }
}
