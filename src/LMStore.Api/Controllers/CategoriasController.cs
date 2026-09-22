using LMStore.Application.DTOs.Catalogo;
using LMStore.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LMStore.Api.Controllers;

[ApiController]
[Route("api/categorias")]
public class CategoriasController(ICatalogoService catalogoService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoriaResponse>>> Listar(CancellationToken ct)
    {
        var categorias = await catalogoService.ListarCategoriasAsync(ct);
        return Ok(categorias);
    }
}
