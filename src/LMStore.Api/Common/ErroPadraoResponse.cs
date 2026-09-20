namespace LMStore.Api.Common;

// Formato padronizado de erro pedido no briefing (seção 21): { status, message, errors }.
public record ErroPadraoResponse(int Status, string Message, IReadOnlyList<string> Errors);
