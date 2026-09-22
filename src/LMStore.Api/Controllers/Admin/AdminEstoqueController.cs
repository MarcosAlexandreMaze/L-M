using LMStore.Application.DTOs.Admin.Estoque;
using LMStore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMStore.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/estoque")]
[Authorize(Roles = "Administrador")]
public class AdminEstoqueController(IAdminEstoqueService adminEstoqueService) : ControllerBase
{
    [HttpGet("{variacaoId:guid}")]
    public async Task<ActionResult<EstoqueResponse>> ObterPorVariacao(Guid variacaoId, CancellationToken ct)
    {
        var estoque = await adminEstoqueService.ObterPorVariacaoAsync(variacaoId, ct);
        return Ok(estoque);
    }

    [HttpPost("{variacaoId:guid}/repor")]
    public async Task<ActionResult<EstoqueResponse>> Repor(Guid variacaoId, ReporEstoqueRequest request, CancellationToken ct)
    {
        var estoque = await adminEstoqueService.ReporAsync(variacaoId, request, ct);
        return Ok(estoque);
    }

    [HttpPost("{variacaoId:guid}/ajustar")]
    public async Task<ActionResult<EstoqueResponse>> Ajustar(Guid variacaoId, AjustarEstoqueRequest request, CancellationToken ct)
    {
        var estoque = await adminEstoqueService.AjustarAsync(variacaoId, request, ct);
        return Ok(estoque);
    }
}
