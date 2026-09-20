using LMStore.Domain.Entities;

namespace LMStore.Domain.Interfaces;

public interface IPedidoRepository
{
    Task<Pedido?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Pedido>> ListarPorClienteAsync(Guid clienteId, int pagina, int tamanhoPagina, CancellationToken ct = default);
    void Adicionar(Pedido pedido);
}
