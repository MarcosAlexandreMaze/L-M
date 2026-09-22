using LMStore.Application.DTOs.Pagamento;

namespace LMStore.Application.Interfaces;

public interface IPaymentGateway
{
    Task<ResultadoPagamento> ProcessarAsync(SolicitacaoPagamento solicitacao, CancellationToken ct = default);
}
