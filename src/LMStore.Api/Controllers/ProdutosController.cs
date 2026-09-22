using LMStore.Application.DTOs.Catalogo;
using LMStore.Application.DTOs.Common;
using LMStore.Application.Interfaces;
using LMStore.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace LMStore.Api.Controllers;

[ApiController]
[Route("api/produtos")]
public class ProdutosController(ICatalogoService catalogoService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProdutoResumoResponse>>> Buscar(
        [FromQuery] string? nome,
        [FromQuery] Guid? categoriaId,
        [FromQuery] Guid? marcaId,
        [FromQuery] GeneroProduto? genero,
        [FromQuery] decimal? precoMin,
        [FromQuery] decimal? precoMax,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken ct = default)
    {
        pagina = Math.Max(pagina, 1);
        tamanhoPagina = Math.Clamp(tamanhoPagina, 1, 100);

        var resultado = await catalogoService.BuscarProdutosAsync(
            nome, categoriaId, marcaId, genero, precoMin, precoMax, pagina, tamanhoPagina, ct);

        return Ok(resultado);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProdutoDetalheResponse>> ObterPorId(Guid id, CancellationToken ct)
    {
        var produto = await catalogoService.ObterProdutoPorIdAsync(id, ct);
        return Ok(produto);
    }
}
