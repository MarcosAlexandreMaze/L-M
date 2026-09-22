namespace LMStore.Application.DTOs.Admin.Produtos;

public record AdicionarVariacaoRequest(
    string Sku,
    string? CodigoBarras,
    string Tamanho,
    string Cor,
    decimal? PrecoAdicional,
    int QuantidadeInicial);
