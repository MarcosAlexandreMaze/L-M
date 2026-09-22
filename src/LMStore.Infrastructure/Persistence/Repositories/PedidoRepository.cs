using LMStore.Domain.Entities;
using LMStore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMStore.Infrastructure.Persistence.Repositories;

public class PedidoRepository(LMStoreDbContext context) : IPedidoRepository
{
    public Task<Pedido?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        context.Pedidos.Include(p => p.Itens).Include(p => p.Pagamentos).FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<(IReadOnlyList<Pedido> Itens, int Total)> ListarPorClienteAsync(
        Guid clienteId, int pagina, int tamanhoPagina, CancellationToken ct = default)
    {
        var consulta = context.Pedidos.Where(p => p.ClienteId == clienteId);

        var total = await consulta.CountAsync(ct);

        var itens = await consulta
            .OrderByDescending(p => p.CriadoEm)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(ct);

        return (itens, total);
    }

    public async Task<(IReadOnlyList<Pedido> Itens, int Total)> ListarTodosAsync(
        int pagina, int tamanhoPagina, CancellationToken ct = default)
    {
        var total = await context.Pedidos.CountAsync(ct);

        var itens = await context.Pedidos
            .OrderByDescending(p => p.CriadoEm)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(ct);

        return (itens, total);
    }

    public void Adicionar(Pedido pedido) => context.Pedidos.Add(pedido);
}
