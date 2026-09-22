using LMStore.Domain.Entities;

namespace LMStore.Domain.Interfaces;

public interface IPedidoRepository
{
    Task<Pedido?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    Task<(IReadOnlyList<Pedido> Itens, int Total)> ListarPorClienteAsync(
        Guid clienteId, int pagina, int tamanhoPagina, CancellationToken ct = default);

    // Só para uso administrativo — lista todos os pedidos, de qualquer cliente.
    Task<(IReadOnlyList<Pedido> Itens, int Total)> ListarTodosAsync(
        int pagina, int tamanhoPagina, CancellationToken ct = default);

    void Adicionar(Pedido pedido);
}
