using LMStore.Application.DTOs.Cupom;
using LMStore.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LMStore.Api.Controllers;

[ApiController]
[Route("api/cupons")]
public class CuponsController(ICupomService cupomService) : ControllerBase
{
    // Sem [Authorize] de propósito: checar se um cupom é válido não é uma operação
    // sensível, e é comum um cliente querer validar um cupom antes mesmo de logar.
    [HttpPost("validar")]
    public async Task<ActionResult<ValidarCupomResponse>> Validar(ValidarCupomRequest request, CancellationToken ct)
    {
        var resultado = await cupomService.ValidarAsync(request, ct);
        return Ok(resultado);
    }
}
