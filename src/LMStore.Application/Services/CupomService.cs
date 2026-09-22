using LMStore.Application.DTOs.Cupom;
using LMStore.Application.Interfaces;
using LMStore.Domain.Interfaces;
using LMStore.Domain.ValueObjects;

namespace LMStore.Application.Services;

public class CupomService(ICupomRepository cupomRepository) : ICupomService
{
    public async Task<ValidarCupomResponse> ValidarAsync(ValidarCupomRequest request, CancellationToken ct = default)
    {
        var cupom = await cupomRepository.ObterPorCodigoAsync(request.Codigo, ct);

        if (cupom is null)
            return new ValidarCupomResponse(false, "Cupom não encontrado.", null);

        var subtotal = new Dinheiro(request.Subtotal);
        var (valido, motivo) = cupom.EstaValidoPara(subtotal, DateTime.UtcNow);

        if (!valido)
            return new ValidarCupomResponse(false, motivo, null);

        return new ValidarCupomResponse(true, null, cupom.CalcularDesconto(subtotal).Valor);
    }
}
