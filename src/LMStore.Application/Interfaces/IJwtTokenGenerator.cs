using LMStore.Domain.Entities;

namespace LMStore.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GerarAccessToken(Usuario usuario);
}
