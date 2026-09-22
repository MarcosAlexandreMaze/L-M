namespace LMStore.Application.DTOs.Common;

public record PagedResult<T>(IReadOnlyList<T> Itens, int Pagina, int TamanhoPagina, int Total);
