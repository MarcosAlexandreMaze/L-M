namespace LMStore.Application.DTOs.Pagamento;

public record ResultadoPagamento(bool Aprovado, string? TransacaoExternaId, string? MotivoRecusa);
