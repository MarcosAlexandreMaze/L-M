using LMStore.Domain.Entities;

namespace LMStore.Domain.Interfaces;

public interface ICarrinhoRepository
{
    Task<Carrinho?> ObterPorClienteIdAsync(Guid clienteId, CancellationToken ct = default);
    void Adicionar(Carrinho carrinho);
}
