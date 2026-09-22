namespace LMStore.Application.DTOs.Catalogo;

public record VariacaoResponse(Guid Id, string Sku, string? CodigoBarras, string Tamanho, string Cor, bool Ativa);
