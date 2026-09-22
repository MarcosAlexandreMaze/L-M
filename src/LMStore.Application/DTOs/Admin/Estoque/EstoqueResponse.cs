namespace LMStore.Application.DTOs.Admin.Estoque;

public record EstoqueResponse(
    Guid Id,
    Guid VariacaoProdutoId,
    int QuantidadeDisponivel,
    int QuantidadeReservada,
    IReadOnlyList<MovimentoEstoqueResponse> Movimentos);
