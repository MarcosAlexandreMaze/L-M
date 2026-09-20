using LMStore.Domain.Entities;
using LMStore.Domain.Interfaces;
using LMStore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace LMStore.Infrastructure.Persistence.Repositories;

public class UsuarioRepository(LMStoreDbContext context) : IUsuarioRepository
{
    public Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        context.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);

    // Inclui RefreshTokens de propósito: o único chamador (login) sempre emite um token
    // novo logo em seguida. Sem o Include, a coleção nunca é "carregada" para o change
    // tracker do EF, e adicionar um item a ela faz o EF tentar um UPDATE num registro
    // que não existe em vez de um INSERT — foi um bug real, pego testando o login de
    // verdade (a criação de conta não expõe isso porque lá o Usuario é todo novo).
    public Task<Usuario?> ObterPorEmailAsync(Email email, CancellationToken ct = default) =>
        context.Usuarios.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<bool> ExistePorEmailAsync(Email email, CancellationToken ct = default) =>
        context.Usuarios.AnyAsync(u => u.Email == email, ct);

    public async Task<Usuario?> ObterPorRefreshTokenHashAsync(string tokenHash, CancellationToken ct = default)
    {
        var usuarioId = await context.Set<RefreshToken>()
            .Where(t => t.TokenHash == tokenHash)
            .Select(t => t.UsuarioId)
            .FirstOrDefaultAsync(ct);

        if (usuarioId == Guid.Empty)
            return null;

        return await context.Usuarios.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Id == usuarioId, ct);
    }

    public void Adicionar(Usuario usuario) => context.Usuarios.Add(usuario);
}
