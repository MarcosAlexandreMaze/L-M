namespace LMStore.Application.DTOs.Carrinho;

public record ItemCarrinhoResponse(
    Guid Id,
    Guid VariacaoProdutoId,
    string NomeProduto,
    string Sku,
    string Tamanho,
    string Cor,
    decimal PrecoUnitario,
    int Quantidade,
    decimal Subtotal);
