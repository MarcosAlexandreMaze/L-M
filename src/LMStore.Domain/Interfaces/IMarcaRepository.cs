using LMStore.Domain.Entities;

namespace LMStore.Domain.Interfaces;

public interface IMarcaRepository
{
    Task<Marca?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Marca>> ListarTodasAsync(CancellationToken ct = default);
    void Adicionar(Marca marca);
}
