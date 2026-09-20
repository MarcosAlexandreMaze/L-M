using LMStore.Domain.Entities;
using LMStore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMStore.Infrastructure.Persistence.Repositories;

public class CupomRepository(LMStoreDbContext context) : ICupomRepository
{
    public Task<Cupom?> ObterPorCodigoAsync(string codigo, CancellationToken ct = default) =>
        context.Cupons.FirstOrDefaultAsync(c => c.Codigo == codigo.Trim().ToUpper(), ct);

    public void Adicionar(Cupom cupom) => context.Cupons.Add(cupom);
}
