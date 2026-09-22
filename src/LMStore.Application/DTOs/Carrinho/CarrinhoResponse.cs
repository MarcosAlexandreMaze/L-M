namespace LMStore.Application.DTOs.Carrinho;

public record CarrinhoResponse(
    Guid Id,
    IReadOnlyList<ItemCarrinhoResponse> Itens,
    decimal Subtotal,
    string? CupomAplicado,
    decimal? Desconto,
    decimal Total);
