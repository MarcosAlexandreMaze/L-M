using LMStore.Application.DTOs.Admin.Estoque;
using LMStore.Application.Interfaces;
using LMStore.Domain.Entities;
using LMStore.Domain.Exceptions;
using LMStore.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace LMStore.Application.Services;

public class AdminEstoqueService(
    IEstoqueRepository estoqueRepository, IUnitOfWork unitOfWork, ILogger<AdminEstoqueService> logger) : IAdminEstoqueService
{
    public async Task<EstoqueResponse> ObterPorVariacaoAsync(Guid variacaoProdutoId, CancellationToken ct = default) =>
        ParaResponse(await ObterOuFalharAsync(variacaoProdutoId, ct));

    public async Task<EstoqueResponse> ReporAsync(
        Guid variacaoProdutoId, ReporEstoqueRequest request, CancellationToken ct = default)
    {
        var estoque = await ObterOuFalharAsync(variacaoProdutoId, ct);
        estoque.Repor(request.Quantidade, request.Motivo);
        await unitOfWork.SalvarAsync(ct);

        logger.LogInformation(
            "Estoque da variação {VariacaoProdutoId} reposto em {Quantidade} unidades. Motivo: {Motivo}",
            variacaoProdutoId, request.Quantidade, request.Motivo);

        return ParaResponse(estoque);
    }

    public async Task<EstoqueResponse> AjustarAsync(
        Guid variacaoProdutoId, AjustarEstoqueRequest request, CancellationToken ct = default)
    {
        var estoque = await ObterOuFalharAsync(variacaoProdutoId, ct);
        estoque.AjustarPara(request.NovaQuantidade, request.Motivo);
        await unitOfWork.SalvarAsync(ct);

        logger.LogInformation(
            "Estoque da variação {VariacaoProdutoId} ajustado para {NovaQuantidade} unidades. Motivo: {Motivo}",
            variacaoProdutoId, request.NovaQuantidade, request.Motivo);

        return ParaResponse(estoque);
    }

    private async Task<Estoque> ObterOuFalharAsync(Guid variacaoProdutoId, CancellationToken ct) =>
        await estoqueRepository.ObterPorVariacaoAsync(variacaoProdutoId, ct)
            ?? throw new NotFoundException($"Estoque não encontrado para a variação '{variacaoProdutoId}'.");

    private static EstoqueResponse ParaResponse(Estoque estoque) => new(
        estoque.Id,
        estoque.VariacaoProdutoId,
        estoque.QuantidadeDisponivel,
        estoque.QuantidadeReservada,
        [.. estoque.Movimentos
            .OrderByDescending(m => m.CriadoEm)
            .Select(m => new MovimentoEstoqueResponse(m.Id, m.Tipo.ToString(), m.Quantidade, m.Motivo, m.PedidoId, m.CriadoEm))]);
}
