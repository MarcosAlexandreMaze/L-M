namespace LMStore.Application.DTOs.Cupom;

public record ValidarCupomResponse(bool Valido, string? Motivo, decimal? Desconto);
