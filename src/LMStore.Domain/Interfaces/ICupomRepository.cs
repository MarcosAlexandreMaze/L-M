using LMStore.Domain.Entities;

namespace LMStore.Domain.Interfaces;

public interface ICupomRepository
{
    Task<Cupom?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Cupom?> ObterPorCodigoAsync(string codigo, CancellationToken ct = default);
    void Adicionar(Cupom cupom);
}
