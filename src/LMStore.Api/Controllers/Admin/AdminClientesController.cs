using LMStore.Application.DTOs.Admin.Clientes;
using LMStore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMStore.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/clientes")]
[Authorize(Roles = "Administrador")]
public class AdminClientesController(IAdminClienteService adminClienteService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClienteAdminResponse>>> Listar(
        [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken ct = default)
    {
        pagina = Math.Max(pagina, 1);
        tamanhoPagina = Math.Clamp(tamanhoPagina, 1, 100);

        var clientes = await adminClienteService.ListarAsync(pagina, tamanhoPagina, ct);
        return Ok(clientes);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClienteAdminResponse>> ObterPorId(Guid id, CancellationToken ct)
    {
        var cliente = await adminClienteService.ObterPorIdAsync(id, ct);
        return Ok(cliente);
    }
}
