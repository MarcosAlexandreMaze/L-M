using LMStore.Domain.Entities;
using LMStore.Domain.ValueObjects;

namespace LMStore.Domain.Interfaces;

public interface IProdutoRepository
{
    Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> SkuJaExisteAsync(Sku sku, CancellationToken ct = default);
    void Adicionar(Produto produto);

    // A busca com filtros combináveis (Specification pattern) entra na Etapa 9, quando
    // existir um consumidor real (o controller de catálogo) para moldar a assinatura —
    // construir isso agora, sem um filtro de verdade pra atender, seria especulativo.
}
