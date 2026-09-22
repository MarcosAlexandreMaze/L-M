using System.IdentityModel.Tokens.Jwt;
using LMStore.Application.Interfaces;

namespace LMStore.Api.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid ObterUsuarioId()
    {
        var sub = httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (sub is null || !Guid.TryParse(sub, out var usuarioId))
            throw new UnauthorizedAccessException("Usuário não autenticado.");

        return usuarioId;
    }
}
