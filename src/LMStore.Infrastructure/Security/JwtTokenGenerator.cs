using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LMStore.Application.Common;
using LMStore.Application.Interfaces;
using LMStore.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LMStore.Infrastructure.Security;

public class JwtTokenGenerator(IOptions<JwtOptions> options) : IJwtTokenGenerator
{
    private readonly JwtOptions _options = options.Value;

    public string GerarAccessToken(Usuario usuario)
    {
        Claim[] claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email.Endereco),
            new Claim(ClaimTypes.Role, usuario.Perfil.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];

        var chaveAssinatura = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credenciais = new SigningCredentials(chaveAssinatura, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutos),
            signingCredentials: credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
