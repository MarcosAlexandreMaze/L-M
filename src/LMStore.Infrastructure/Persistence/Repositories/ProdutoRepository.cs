using LMStore.Domain.Entities;
using LMStore.Domain.Interfaces;
using LMStore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace LMStore.Infrastructure.Persistence.Repositories;

public class ProdutoRepository(LMStoreDbContext context) : IProdutoRepository
{
    public Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        context.Produtos.Include(p => p.Variacoes).FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<bool> SkuJaExisteAsync(Sku sku, CancellationToken ct = default) =>
        context.Set<VariacaoProduto>().AnyAsync(v => v.Sku == sku, ct);

    public void Adicionar(Produto produto) => context.Produtos.Add(produto);
}
