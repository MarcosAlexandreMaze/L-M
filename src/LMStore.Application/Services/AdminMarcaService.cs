using LMStore.Application.DTOs.Admin.Marcas;
using LMStore.Application.DTOs.Catalogo;
using LMStore.Application.Interfaces;
using LMStore.Application.Mappings;
using LMStore.Domain.Entities;
using LMStore.Domain.Exceptions;
using LMStore.Domain.Interfaces;

namespace LMStore.Application.Services;

public class AdminMarcaService(IMarcaRepository marcaRepository, IUnitOfWork unitOfWork) : IAdminMarcaService
{
    public async Task<MarcaResponse> CriarAsync(CriarMarcaRequest request, CancellationToken ct = default)
    {
        // Marcas.Nome tem índice único no banco (Etapa 6) — esta checagem é só pra dar
        // um erro de negócio claro em vez de deixar a exceção crua do SQL vazar.
        if (await ExisteComNomeAsync(request.Nome, ct))
            throw new ConflictException($"Já existe uma marca com o nome '{request.Nome}'.");

        var marca = new Marca(request.Nome);
        marcaRepository.Adicionar(marca);
        await unitOfWork.SalvarAsync(ct);

        return marca.ParaResponse();
    }

    public async Task<MarcaResponse> RenomearAsync(Guid marcaId, RenomearMarcaRequest request, CancellationToken ct = default)
    {
        var marca = await ObterOuFalharAsync(marcaId, ct);

        if (await ExisteComNomeAsync(request.Nome, ct, ignorarId: marcaId))
            throw new ConflictException($"Já existe uma marca com o nome '{request.Nome}'.");

        marca.Renomear(request.Nome);
        await unitOfWork.SalvarAsync(ct);

        return marca.ParaResponse();
    }

    public async Task<MarcaResponse> AtivarAsync(Guid marcaId, CancellationToken ct = default)
    {
        var marca = await ObterOuFalharAsync(marcaId, ct);
        marca.Ativar();
        await unitOfWork.SalvarAsync(ct);

        return marca.ParaResponse();
    }

    public async Task<MarcaResponse> DesativarAsync(Guid marcaId, CancellationToken ct = default)
    {
        var marca = await ObterOuFalharAsync(marcaId, ct);
        marca.Desativar();
        await unitOfWork.SalvarAsync(ct);

        return marca.ParaResponse();
    }

    private async Task<bool> ExisteComNomeAsync(string nome, CancellationToken ct, Guid? ignorarId = null) =>
        (await marcaRepository.ListarTodasAsync(ct))
            .Any(m => m.Id != ignorarId && m.Nome.Equals(nome.Trim(), StringComparison.OrdinalIgnoreCase));

    private async Task<Marca> ObterOuFalharAsync(Guid marcaId, CancellationToken ct) =>
        await marcaRepository.ObterPorIdAsync(marcaId, ct)
            ?? throw new NotFoundException($"Marca '{marcaId}' não encontrada.");
}
