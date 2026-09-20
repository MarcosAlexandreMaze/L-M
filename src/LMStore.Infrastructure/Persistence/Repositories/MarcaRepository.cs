using LMStore.Domain.Entities;
using LMStore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMStore.Infrastructure.Persistence.Repositories;

public class MarcaRepository(LMStoreDbContext context) : IMarcaRepository
{
    public Task<Marca?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        context.Marcas.FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<IReadOnlyList<Marca>> ListarTodasAsync(CancellationToken ct = default) =>
        await context.Marcas.OrderBy(m => m.Nome).ToListAsync(ct);

    public void Adicionar(Marca marca) => context.Marcas.Add(marca);
}
