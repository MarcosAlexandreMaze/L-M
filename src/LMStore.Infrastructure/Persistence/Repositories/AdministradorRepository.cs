using LMStore.Domain.Entities;
using LMStore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMStore.Infrastructure.Persistence.Repositories;

public class AdministradorRepository(LMStoreDbContext context) : IAdministradorRepository
{
    public Task<Administrador?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        context.Administradores.FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<Administrador?> ObterPorUsuarioIdAsync(Guid usuarioId, CancellationToken ct = default) =>
        context.Administradores.FirstOrDefaultAsync(a => a.UsuarioId == usuarioId, ct);

    public void Adicionar(Administrador administrador) => context.Administradores.Add(administrador);
}
