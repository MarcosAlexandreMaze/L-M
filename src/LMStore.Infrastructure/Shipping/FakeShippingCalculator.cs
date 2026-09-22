using LMStore.Application.Interfaces;
using LMStore.Domain.ValueObjects;

namespace LMStore.Infrastructure.Shipping;

// Stub — sem integração real com Correios/transportadora ainda. Varia um pouco pela
// região do CEP só para o valor não ser sempre idêntico; troque por uma implementação
// real quando houver um provedor de frete integrado.
public class FakeShippingCalculator : IShippingCalculator
{
    private const decimal TaxaBase = 15m;
    private const decimal AdicionalPorRegiao = 1.5m;

    public Task<decimal> CalcularAsync(Cep cepDestino, CancellationToken ct = default)
    {
        var primeiroDigito = cepDestino.Numero[0] - '0';
        return Task.FromResult(TaxaBase + (primeiroDigito * AdicionalPorRegiao));
    }
}
