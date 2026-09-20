using LMStore.Domain.Entities;
using LMStore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMStore.Infrastructure.Persistence.Repositories;

public class PedidoRepository(LMStoreDbContext context) : IPedidoRepository
{
    public Task<Pedido?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        context.Pedidos.Include(p => p.Itens).Include(p => p.Pagamentos).FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Pedido>> ListarPorClienteAsync(
        Guid clienteId, int pagina, int tamanhoPagina, CancellationToken ct = default) =>
        await context.Pedidos
            .Where(p => p.ClienteId == clienteId)
            .OrderByDescending(p => p.CriadoEm)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(ct);

    public void Adicionar(Pedido pedido) => context.Pedidos.Add(pedido);
}
