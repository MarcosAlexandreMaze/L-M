using LMStore.Domain.Entities;
using LMStore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMStore.Infrastructure.Persistence.Repositories;

public class CarrinhoRepository(LMStoreDbContext context) : ICarrinhoRepository
{
    public Task<Carrinho?> ObterPorClienteIdAsync(Guid clienteId, CancellationToken ct = default) =>
        context.Carrinhos.Include(c => c.Itens).FirstOrDefaultAsync(c => c.ClienteId == clienteId, ct);

    public void Adicionar(Carrinho carrinho) => context.Carrinhos.Add(carrinho);
}
