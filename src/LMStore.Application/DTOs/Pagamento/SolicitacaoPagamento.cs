using LMStore.Domain.Enums;

namespace LMStore.Application.DTOs.Pagamento;

// TokenPagamento é opaco de propósito — nenhum dado sensível de cartão (PAN/CVV) tem
// espaço para existir nesta assinatura. O token representa o que o SDK client-side de
// um gateway real (Stripe, PagSeguro, Mercado Pago...) já teria tokenizado antes de
// chegar aqui — a restrição de segurança está no próprio formato do contrato, não
// depende de disciplina de quem chama.
public record SolicitacaoPagamento(Guid PedidoId, FormaPagamento FormaPagamento, decimal Valor, string TokenPagamento);
