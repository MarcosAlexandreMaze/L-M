using LMStore.Application.DTOs.Auth;

namespace LMStore.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResultado> RegistrarClienteAsync(RegistrarClienteRequest request, CancellationToken ct = default);
    Task<AuthResultado> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthResultado> RefreshAsync(string refreshTokenBruto, CancellationToken ct = default);
}
