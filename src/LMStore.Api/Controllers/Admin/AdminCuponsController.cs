using LMStore.Application.DTOs.Admin.Cupons;
using LMStore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMStore.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/cupons")]
[Authorize(Roles = "Administrador")]
public class AdminCuponsController(IAdminCupomService adminCupomService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CupomResponse>> Criar(CriarCupomRequest request, CancellationToken ct)
    {
        var cupom = await adminCupomService.CriarAsync(request, ct);
        return StatusCode(201, cupom);
    }

    [HttpPost("{id:guid}/ativar")]
    public async Task<ActionResult<CupomResponse>> Ativar(Guid id, CancellationToken ct)
    {
        var cupom = await adminCupomService.AtivarAsync(id, ct);
        return Ok(cupom);
    }

    [HttpPost("{id:guid}/desativar")]
    public async Task<ActionResult<CupomResponse>> Desativar(Guid id, CancellationToken ct)
    {
        var cupom = await adminCupomService.DesativarAsync(id, ct);
        return Ok(cupom);
    }
}
