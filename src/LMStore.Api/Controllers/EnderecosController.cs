using LMStore.Application.DTOs.Cliente;
using LMStore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMStore.Api.Controllers;

[ApiController]
[Route("api/clientes/enderecos")]
[Authorize(Roles = "Cliente")]
public class EnderecosController(IClienteService clienteService, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EnderecoResponse>>> Listar(CancellationToken ct)
    {
        var enderecos = await clienteService.ListarEnderecosAsync(currentUserService.ObterUsuarioId(), ct);
        return Ok(enderecos);
    }

    [HttpPost]
    public async Task<ActionResult<EnderecoResponse>> Adicionar(EnderecoRequest request, CancellationToken ct)
    {
        var endereco = await clienteService.AdicionarEnderecoAsync(currentUserService.ObterUsuarioId(), request, ct);
        return StatusCode(201, endereco);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EnderecoResponse>> Atualizar(Guid id, EnderecoRequest request, CancellationToken ct)
    {
        var endereco = await clienteService.AtualizarEnderecoAsync(currentUserService.ObterUsuarioId(), id, request, ct);
        return Ok(endereco);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id, CancellationToken ct)
    {
        await clienteService.RemoverEnderecoAsync(currentUserService.ObterUsuarioId(), id, ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/padrao")]
    public async Task<ActionResult<EnderecoResponse>> DefinirPadrao(Guid id, CancellationToken ct)
    {
        var endereco = await clienteService.DefinirEnderecoPadraoAsync(currentUserService.ObterUsuarioId(), id, ct);
        return Ok(endereco);
    }
}
