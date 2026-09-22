using LMStore.Application.DTOs.Admin.Categorias;
using LMStore.Application.DTOs.Catalogo;
using LMStore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMStore.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/categorias")]
[Authorize(Roles = "Administrador")]
public class AdminCategoriasController(IAdminCategoriaService adminCategoriaService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CategoriaResponse>> Criar(CriarCategoriaRequest request, CancellationToken ct)
    {
        var categoria = await adminCategoriaService.CriarAsync(request, ct);
        return StatusCode(201, categoria);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CategoriaResponse>> Renomear(Guid id, RenomearCategoriaRequest request, CancellationToken ct)
    {
        var categoria = await adminCategoriaService.RenomearAsync(id, request, ct);
        return Ok(categoria);
    }

    [HttpPut("{id:guid}/mover")]
    public async Task<ActionResult<CategoriaResponse>> Mover(Guid id, MoverCategoriaRequest request, CancellationToken ct)
    {
        var categoria = await adminCategoriaService.MoverAsync(id, request, ct);
        return Ok(categoria);
    }

    [HttpPost("{id:guid}/ativar")]
    public async Task<ActionResult<CategoriaResponse>> Ativar(Guid id, CancellationToken ct)
    {
        var categoria = await adminCategoriaService.AtivarAsync(id, ct);
        return Ok(categoria);
    }

    [HttpPost("{id:guid}/desativar")]
    public async Task<ActionResult<CategoriaResponse>> Desativar(Guid id, CancellationToken ct)
    {
        var categoria = await adminCategoriaService.DesativarAsync(id, ct);
        return Ok(categoria);
    }
}
