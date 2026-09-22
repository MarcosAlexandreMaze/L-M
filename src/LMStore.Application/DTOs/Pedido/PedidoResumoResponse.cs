namespace LMStore.Application.DTOs.Pedido;

public record PedidoResumoResponse(Guid Id, string Numero, string Status, decimal Total, DateTime CriadoEm);
