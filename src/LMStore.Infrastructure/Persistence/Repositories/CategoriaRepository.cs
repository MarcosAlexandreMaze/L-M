using LMStore.Domain.Entities;
using LMStore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMStore.Infrastructure.Persistence.Repositories;

public class CategoriaRepository(LMStoreDbContext context) : ICategoriaRepository
{
    public Task<Categoria?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        context.Categorias.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Categoria>> ListarTodasAsync(CancellationToken ct = default) =>
        await context.Categorias.OrderBy(c => c.Nome).ToListAsync(ct);

    public void Adicionar(Categoria categoria) => context.Categorias.Add(categoria);
}
