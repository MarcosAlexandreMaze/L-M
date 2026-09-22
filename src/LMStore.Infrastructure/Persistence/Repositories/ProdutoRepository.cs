using LMStore.Domain.Entities;
using LMStore.Domain.Interfaces;
using LMStore.Domain.Specifications;
using LMStore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace LMStore.Infrastructure.Persistence.Repositories;

public class ProdutoRepository(LMStoreDbContext context) : IProdutoRepository
{
    public Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        context.Produtos.Include(p => p.Variacoes).FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<bool> SkuJaExisteAsync(Sku sku, CancellationToken ct = default) =>
        context.Set<VariacaoProduto>().AnyAsync(v => v.Sku == sku, ct);

    public async Task<(IReadOnlyList<Produto> Itens, int Total)> BuscarAsync(
        ISpecification<Produto> especificacao, int pagina, int tamanhoPagina, CancellationToken ct = default)
    {
        var consulta = context.Produtos.Include(p => p.Variacoes).AsQueryable();

        if (especificacao.Criteria is not null)
            consulta = consulta.Where(especificacao.Criteria);

        var total = await consulta.CountAsync(ct);

        var itens = await consulta
            .OrderBy(p => p.Nome)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(ct);

        return (itens, total);
    }

    public async Task<IReadOnlyList<Produto>> ObterPorVariacaoIdsAsync(
        IEnumerable<Guid> variacaoIds, CancellationToken ct = default)
    {
        var ids = variacaoIds.ToList();

        if (ids.Count == 0)
            return [];

        return await context.Produtos
            .Include(p => p.Variacoes)
            .Where(p => p.Variacoes.Any(v => ids.Contains(v.Id)))
            .ToListAsync(ct);
    }

    public void Adicionar(Produto produto) => context.Produtos.Add(produto);
}
