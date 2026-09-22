using LMStore.Domain.Entities;
using LMStore.Domain.Specifications;
using LMStore.Domain.ValueObjects;

namespace LMStore.Domain.Interfaces;

public interface IProdutoRepository
{
    Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> SkuJaExisteAsync(Sku sku, CancellationToken ct = default);

    Task<(IReadOnlyList<Produto> Itens, int Total)> BuscarAsync(
        ISpecification<Produto> especificacao, int pagina, int tamanhoPagina, CancellationToken ct = default);

    // ItemCarrinho/ItemPedido só guardam o Id da variação (de propósito, ver Etapa 5/6) —
    // para exibir nome/SKU/tamanho no carrinho, é preciso resolver o Produto dono de um
    // conjunto de variações. Retorna os Produtos (com Variacoes incluídas), não um DTO,
    // pra continuar reaproveitável por qualquer camada que precise dessa mesma consulta.
    Task<IReadOnlyList<Produto>> ObterPorVariacaoIdsAsync(IEnumerable<Guid> variacaoIds, CancellationToken ct = default);

    void Adicionar(Produto produto);
}
