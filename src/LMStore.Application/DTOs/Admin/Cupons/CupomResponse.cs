namespace LMStore.Application.DTOs.Admin.Cupons;

public record CupomResponse(
    Guid Id,
    string Codigo,
    string TipoDesconto,
    decimal Valor,
    DateTime DataInicio,
    DateTime DataFim,
    decimal ValorMinimoCompra,
    int LimiteUso,
    int QuantidadeUtilizada,
    bool Ativo);
