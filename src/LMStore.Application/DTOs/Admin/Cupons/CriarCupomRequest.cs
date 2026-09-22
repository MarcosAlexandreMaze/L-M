using LMStore.Domain.Enums;

namespace LMStore.Application.DTOs.Admin.Cupons;

public record CriarCupomRequest(
    string Codigo,
    TipoDesconto TipoDesconto,
    decimal Valor,
    DateTime DataInicio,
    DateTime DataFim,
    decimal ValorMinimoCompra,
    int LimiteUso);
