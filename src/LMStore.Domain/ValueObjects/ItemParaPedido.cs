namespace LMStore.Domain.ValueObjects;

public sealed record ItemParaPedido(
    Guid VariacaoProdutoId, string NomeProduto, Sku Sku, Dinheiro PrecoUnitario, int Quantidade);
