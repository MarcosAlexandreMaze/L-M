namespace LMStore.Application.DTOs.Admin.Pedidos;

public record PedidoAdminResumoResponse(Guid Id, string Numero, Guid ClienteId, string Status, decimal Total, DateTime CriadoEm);
