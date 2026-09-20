using LMStore.Domain.Entities;
using LMStore.Domain.ValueObjects;

namespace LMStore.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Usuario?> ObterPorEmailAsync(Email email, CancellationToken ct = default);
    Task<Usuario?> ObterPorRefreshTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task<bool> ExistePorEmailAsync(Email email, CancellationToken ct = default);
    void Adicionar(Usuario usuario);
}
