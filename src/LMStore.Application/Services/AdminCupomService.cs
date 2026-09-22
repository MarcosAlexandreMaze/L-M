using LMStore.Application.DTOs.Admin.Cupons;
using LMStore.Application.Interfaces;
using LMStore.Domain.Entities;
using LMStore.Domain.Exceptions;
using LMStore.Domain.Interfaces;
using LMStore.Domain.ValueObjects;

namespace LMStore.Application.Services;

public class AdminCupomService(ICupomRepository cupomRepository, IUnitOfWork unitOfWork) : IAdminCupomService
{
    public async Task<CupomResponse> CriarAsync(CriarCupomRequest request, CancellationToken ct = default)
    {
        if (await cupomRepository.ObterPorCodigoAsync(request.Codigo, ct) is not null)
            throw new ConflictException($"Já existe um cupom com o código '{request.Codigo}'.");

        var cupom = new Cupom(
            request.Codigo, request.TipoDesconto, request.Valor, request.DataInicio, request.DataFim,
            new Dinheiro(request.ValorMinimoCompra), request.LimiteUso);

        cupomRepository.Adicionar(cupom);
        await unitOfWork.SalvarAsync(ct);

        return ParaResponse(cupom);
    }

    public async Task<CupomResponse> AtivarAsync(Guid cupomId, CancellationToken ct = default)
    {
        var cupom = await ObterOuFalharAsync(cupomId, ct);
        cupom.Ativar();
        await unitOfWork.SalvarAsync(ct);

        return ParaResponse(cupom);
    }

    public async Task<CupomResponse> DesativarAsync(Guid cupomId, CancellationToken ct = default)
    {
        var cupom = await ObterOuFalharAsync(cupomId, ct);
        cupom.Desativar();
        await unitOfWork.SalvarAsync(ct);

        return ParaResponse(cupom);
    }

    private async Task<Cupom> ObterOuFalharAsync(Guid cupomId, CancellationToken ct) =>
        await cupomRepository.ObterPorIdAsync(cupomId, ct)
            ?? throw new NotFoundException($"Cupom '{cupomId}' não encontrado.");

    private static CupomResponse ParaResponse(Cupom cupom) => new(
        cupom.Id, cupom.Codigo, cupom.TipoDesconto.ToString(), cupom.Valor, cupom.DataInicio, cupom.DataFim,
        cupom.ValorMinimoCompra.Valor, cupom.LimiteUso, cupom.QuantidadeUtilizada, cupom.Ativo);
}
