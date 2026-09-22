using LMStore.Domain.Enums;

namespace LMStore.Application.DTOs.Pedido;

public record CheckoutRequest(
    Guid EnderecoEntregaId,
    FormaPagamento FormaPagamento,
    string TokenPagamento,
    string? CodigoCupom);
