namespace LMStore.Application.DTOs.Pedido;

public record PedidoItemResponse(Guid Id, string NomeProduto, string Sku, decimal PrecoUnitario, int Quantidade, decimal Subtotal);
