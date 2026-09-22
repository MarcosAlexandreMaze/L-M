using LMStore.Application.DTOs.Admin.Pedidos;
using LMStore.Application.DTOs.Common;
using LMStore.Application.DTOs.Pedido;
using LMStore.Application.Interfaces;
using LMStore.Application.Mappings;
using LMStore.Domain.Entities;
using LMStore.Domain.Exceptions;
using LMStore.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace LMStore.Application.Services;

public class AdminPedidoService(
    IPedidoRepository pedidoRepository,
    IEstoqueRepository estoqueRepository,
    IUnitOfWork unitOfWork,
    ILogger<AdminPedidoService> logger) : IAdminPedidoService
{
    public async Task<PagedResult<PedidoAdminResumoResponse>> ListarTodosAsync(
        int pagina, int tamanhoPagina, CancellationToken ct = default)
    {
        var (pedidos, total) = await pedidoRepository.ListarTodosAsync(pagina, tamanhoPagina, ct);

        var itens = pedidos
            .Select(p => new PedidoAdminResumoResponse(p.Id, p.Numero, p.ClienteId, p.Status.ToString(), p.Total.Valor, p.CriadoEm))
            .ToList();

        return new PagedResult<PedidoAdminResumoResponse>(itens, pagina, tamanhoPagina, total);
    }

    public async Task<PedidoResponse> ObterPorIdAsync(Guid pedidoId, CancellationToken ct = default) =>
        (await ObterOuFalharAsync(pedidoId, ct)).ParaResponse();

    public async Task<PedidoResponse> IniciarPreparacaoAsync(Guid pedidoId, CancellationToken ct = default)
    {
        var pedido = await ObterOuFalharAsync(pedidoId, ct);
        pedido.IniciarPreparacao();
        await unitOfWork.SalvarAsync(ct);

        logger.LogInformation("Pedido {PedidoId} teve preparação iniciada pelo admin", pedido.Id);

        return pedido.ParaResponse();
    }

    public async Task<PedidoResponse> MarcarComoEnviadoAsync(
        Guid pedidoId, MarcarComoEnviadoRequest request, CancellationToken ct = default)
    {
        var pedido = await ObterOuFalharAsync(pedidoId, ct);
        pedido.MarcarComoEnviado(request.CodigoRastreio);

        // Só agora a baixa de estoque se torna definitiva: a reserva feita no checkout
        // (Etapa 11) vira saída de verdade. Antes disso, a mercadoria ainda não saiu
        // fisicamente do estoque — continua "reservada", não "baixada" (decisão da
        // Etapa 5, implementada na Etapa 7, e só agora de fato chamada por alguém).
        await unitOfWork.ExecutarEmTransacaoAsync(async () =>
        {
            foreach (var item in pedido.Itens)
                await estoqueRepository.ConfirmarSaidaAsync(item.VariacaoProdutoId, item.Quantidade, pedido.Id, ct);

            await unitOfWork.SalvarAsync(ct);
        }, ct);

        logger.LogInformation(
            "Pedido {PedidoId} marcado como enviado — rastreio {CodigoRastreio}", pedido.Id, request.CodigoRastreio);

        return pedido.ParaResponse();
    }

    public async Task<PedidoResponse> MarcarComoEntregueAsync(Guid pedidoId, CancellationToken ct = default)
    {
        var pedido = await ObterOuFalharAsync(pedidoId, ct);
        pedido.MarcarComoEntregue();
        await unitOfWork.SalvarAsync(ct);

        logger.LogInformation("Pedido {PedidoId} marcado como entregue", pedido.Id);

        return pedido.ParaResponse();
    }

    public async Task<PedidoResponse> CancelarAsync(Guid pedidoId, CancelarPedidoRequest request, CancellationToken ct = default)
    {
        var pedido = await ObterOuFalharAsync(pedidoId, ct);
        pedido.Cancelar(request.Motivo);

        await unitOfWork.ExecutarEmTransacaoAsync(async () =>
        {
            foreach (var item in pedido.Itens)
                await estoqueRepository.CancelarReservaAsync(item.VariacaoProdutoId, item.Quantidade, pedido.Id, ct);

            await unitOfWork.SalvarAsync(ct);
        }, ct);

        logger.LogInformation("Pedido {PedidoId} cancelado pelo admin. Motivo: {Motivo}", pedido.Id, request.Motivo);

        return pedido.ParaResponse();
    }

    private async Task<Pedido> ObterOuFalharAsync(Guid pedidoId, CancellationToken ct) =>
        await pedidoRepository.ObterPorIdAsync(pedidoId, ct)
            ?? throw new NotFoundException($"Pedido '{pedidoId}' não encontrado.");
}
