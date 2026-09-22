using LMStore.Application.DTOs.Cupom;

namespace LMStore.Application.Interfaces;

public interface ICupomService
{
    Task<ValidarCupomResponse> ValidarAsync(ValidarCupomRequest request, CancellationToken ct = default);
}
