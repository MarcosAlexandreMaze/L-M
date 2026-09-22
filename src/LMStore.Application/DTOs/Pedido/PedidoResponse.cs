namespace LMStore.Application.DTOs.Pedido;

public record PedidoResponse(
    Guid Id,
    string Numero,
    string Status,
    IReadOnlyList<PedidoItemResponse> Itens,
    decimal Subtotal,
    decimal DescontoCupom,
    decimal ValorFrete,
    decimal Total,
    string FormaPagamento,
    string StatusPagamento,
    string? MotivoCancelamento,
    EnderecoEntregaResponse EnderecoEntrega,
    DateTime CriadoEm);
