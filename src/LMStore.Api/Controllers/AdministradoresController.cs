using LMStore.Application.DTOs.Admin.Administradores;
using LMStore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMStore.Api.Controllers;

// Fora de Controllers/Admin/ de propósito: não é "gerenciar a loja", é "gerenciar quem
// pode gerenciar a loja" — ainda assim, só um Administrador já autenticado consegue
// criar outro. Não existe nenhum caminho público para se tornar admin.
[ApiController]
[Route("api/administradores")]
[Authorize(Roles = "Administrador")]
public class AdministradoresController(IAdministradorService administradorService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<AdministradorResponse>> Criar(CriarAdministradorRequest request, CancellationToken ct)
    {
        var administrador = await administradorService.CriarAsync(request, ct);
        return StatusCode(201, administrador);
    }
}
