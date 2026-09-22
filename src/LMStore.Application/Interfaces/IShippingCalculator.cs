using LMStore.Domain.ValueObjects;

namespace LMStore.Application.Interfaces;

public interface IShippingCalculator
{
    Task<decimal> CalcularAsync(Cep cepDestino, CancellationToken ct = default);
}
