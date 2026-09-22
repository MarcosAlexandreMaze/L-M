using LMStore.Application.DTOs.Admin.Marcas;
using LMStore.Application.DTOs.Catalogo;
using LMStore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMStore.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/marcas")]
[Authorize(Roles = "Administrador")]
public class AdminMarcasController(IAdminMarcaService adminMarcaService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<MarcaResponse>> Criar(CriarMarcaRequest request, CancellationToken ct)
    {
        var marca = await adminMarcaService.CriarAsync(request, ct);
        return StatusCode(201, marca);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<MarcaResponse>> Renomear(Guid id, RenomearMarcaRequest request, CancellationToken ct)
    {
        var marca = await adminMarcaService.RenomearAsync(id, request, ct);
        return Ok(marca);
    }

    [HttpPost("{id:guid}/ativar")]
    public async Task<ActionResult<MarcaResponse>> Ativar(Guid id, CancellationToken ct)
    {
        var marca = await adminMarcaService.AtivarAsync(id, ct);
        return Ok(marca);
    }

    [HttpPost("{id:guid}/desativar")]
    public async Task<ActionResult<MarcaResponse>> Desativar(Guid id, CancellationToken ct)
    {
        var marca = await adminMarcaService.DesativarAsync(id, ct);
        return Ok(marca);
    }
}
