using LMStore.Application.DTOs.Carrinho;
using LMStore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMStore.Api.Controllers;

[ApiController]
[Route("api/carrinho")]
[Authorize(Roles = "Cliente")]
public class CarrinhoController(ICarrinhoService carrinhoService, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CarrinhoResponse>> Obter(CancellationToken ct)
    {
        var carrinho = await carrinhoService.ObterAsync(currentUserService.ObterUsuarioId(), ct);
        return Ok(carrinho);
    }

    [HttpPost("itens")]
    public async Task<ActionResult<CarrinhoResponse>> AdicionarItem(AdicionarItemRequest request, CancellationToken ct)
    {
        var carrinho = await carrinhoService.AdicionarItemAsync(currentUserService.ObterUsuarioId(), request, ct);
        return Ok(carrinho);
    }

    [HttpPut("itens/{id:guid}")]
    public async Task<ActionResult<CarrinhoResponse>> AlterarQuantidade(
        Guid id, AlterarQuantidadeRequest request, CancellationToken ct)
    {
        var carrinho = await carrinhoService.AlterarQuantidadeAsync(currentUserService.ObterUsuarioId(), id, request, ct);
        return Ok(carrinho);
    }

    [HttpDelete("itens/{id:guid}")]
    public async Task<ActionResult<CarrinhoResponse>> RemoverItem(Guid id, CancellationToken ct)
    {
        var carrinho = await carrinhoService.RemoverItemAsync(currentUserService.ObterUsuarioId(), id, ct);
        return Ok(carrinho);
    }

    [HttpPost("cupom")]
    public async Task<ActionResult<CarrinhoResponse>> AplicarCupom(AplicarCupomRequest request, CancellationToken ct)
    {
        var carrinho = await carrinhoService.AplicarCupomAsync(currentUserService.ObterUsuarioId(), request, ct);
        return Ok(carrinho);
    }

    [HttpDelete("cupom")]
    public async Task<ActionResult<CarrinhoResponse>> RemoverCupom(CancellationToken ct)
    {
        var carrinho = await carrinhoService.RemoverCupomAsync(currentUserService.ObterUsuarioId(), ct);
        return Ok(carrinho);
    }
}
