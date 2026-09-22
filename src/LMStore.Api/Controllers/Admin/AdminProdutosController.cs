using LMStore.Application.DTOs.Admin.Produtos;
using LMStore.Application.DTOs.Catalogo;
using LMStore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMStore.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/produtos")]
[Authorize(Roles = "Administrador")]
public class AdminProdutosController(IAdminProdutoService adminProdutoService) : ControllerBase
{
    [HttpPost("roupas")]
    public async Task<ActionResult<ProdutoDetalheResponse>> CriarRoupa(CriarRoupaRequest request, CancellationToken ct)
    {
        var produto = await adminProdutoService.CriarRoupaAsync(request, ct);
        return StatusCode(201, produto);
    }

    [HttpPost("tenis")]
    public async Task<ActionResult<ProdutoDetalheResponse>> CriarTenis(CriarTenisRequest request, CancellationToken ct)
    {
        var produto = await adminProdutoService.CriarTenisAsync(request, ct);
        return StatusCode(201, produto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProdutoDetalheResponse>> Atualizar(Guid id, AtualizarProdutoRequest request, CancellationToken ct)
    {
        var produto = await adminProdutoService.AtualizarAsync(id, request, ct);
        return Ok(produto);
    }

    [HttpPost("{id:guid}/promocao")]
    public async Task<ActionResult<ProdutoDetalheResponse>> AplicarPromocao(
        Guid id, AplicarPromocaoRequest request, CancellationToken ct)
    {
        var produto = await adminProdutoService.AplicarPromocaoAsync(id, request, ct);
        return Ok(produto);
    }

    [HttpDelete("{id:guid}/promocao")]
    public async Task<ActionResult<ProdutoDetalheResponse>> RemoverPromocao(Guid id, CancellationToken ct)
    {
        var produto = await adminProdutoService.RemoverPromocaoAsync(id, ct);
        return Ok(produto);
    }

    [HttpPost("{id:guid}/ativar")]
    public async Task<ActionResult<ProdutoDetalheResponse>> Ativar(Guid id, CancellationToken ct)
    {
        var produto = await adminProdutoService.AtivarAsync(id, ct);
        return Ok(produto);
    }

    [HttpPost("{id:guid}/desativar")]
    public async Task<ActionResult<ProdutoDetalheResponse>> Desativar(Guid id, CancellationToken ct)
    {
        var produto = await adminProdutoService.DesativarAsync(id, ct);
        return Ok(produto);
    }

    [HttpPost("{id:guid}/variacoes")]
    public async Task<ActionResult<ProdutoDetalheResponse>> AdicionarVariacao(
        Guid id, AdicionarVariacaoRequest request, CancellationToken ct)
    {
        var produto = await adminProdutoService.AdicionarVariacaoAsync(id, request, ct);
        return StatusCode(201, produto);
    }

    [HttpPost("{id:guid}/variacoes/{variacaoId:guid}/ativar")]
    public async Task<ActionResult<ProdutoDetalheResponse>> AtivarVariacao(Guid id, Guid variacaoId, CancellationToken ct)
    {
        var produto = await adminProdutoService.AtivarVariacaoAsync(id, variacaoId, ct);
        return Ok(produto);
    }

    [HttpPost("{id:guid}/variacoes/{variacaoId:guid}/desativar")]
    public async Task<ActionResult<ProdutoDetalheResponse>> DesativarVariacao(Guid id, Guid variacaoId, CancellationToken ct)
    {
        var produto = await adminProdutoService.DesativarVariacaoAsync(id, variacaoId, ct);
        return Ok(produto);
    }
}
