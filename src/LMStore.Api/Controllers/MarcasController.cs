using LMStore.Application.DTOs.Catalogo;
using LMStore.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LMStore.Api.Controllers;

[ApiController]
[Route("api/marcas")]
public class MarcasController(ICatalogoService catalogoService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MarcaResponse>>> Listar(CancellationToken ct)
    {
        var marcas = await catalogoService.ListarMarcasAsync(ct);
        return Ok(marcas);
    }
}
