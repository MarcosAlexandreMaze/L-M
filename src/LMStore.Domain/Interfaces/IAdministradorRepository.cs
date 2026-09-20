using LMStore.Domain.Entities;

namespace LMStore.Domain.Interfaces;

public interface IAdministradorRepository
{
    Task<Administrador?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Administrador?> ObterPorUsuarioIdAsync(Guid usuarioId, CancellationToken ct = default);
    void Adicionar(Administrador administrador);
}
