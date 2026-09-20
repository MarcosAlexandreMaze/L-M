using LMStore.Domain.Entities;
using LMStore.Domain.ValueObjects;

namespace LMStore.Domain.Interfaces;

public interface IClienteRepository
{
    Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Cliente?> ObterPorIdComEnderecosAsync(Guid id, CancellationToken ct = default);
    Task<Cliente?> ObterPorUsuarioIdAsync(Guid usuarioId, CancellationToken ct = default);
    Task<bool> ExistePorCpfAsync(Cpf cpf, CancellationToken ct = default);
    Task<IReadOnlyList<Cliente>> ListarAsync(int pagina, int tamanhoPagina, CancellationToken ct = default);
    void Adicionar(Cliente cliente);
}
