namespace LMStore.Application.DTOs.Admin.Estoque;

public record MovimentoEstoqueResponse(Guid Id, string Tipo, int Quantidade, string Motivo, Guid? PedidoId, DateTime CriadoEm);
