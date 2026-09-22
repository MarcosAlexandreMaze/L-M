using LMStore.Application.DTOs.Pagamento;
using LMStore.Application.Interfaces;

namespace LMStore.Infrastructure.Payments;

// Stub — sem gateway real conectado ainda (PIX/cartão de verdade ficam para quando
// houver um provedor real integrado). Aprova por padrão; recusa só quando o token for
// literalmente "TOKEN_RECUSAR", para dar um jeito determinístico de testar os dois
// caminhos (Pedido.ConfirmarPagamento / Pedido.RecusarPagamento) sem depender de nada
// externo. Trocar por uma implementação real de IPaymentGateway não exige tocar em
// nenhuma regra de negócio — é exatamente o que essa interface existe para permitir.
public class FakePaymentGateway : IPaymentGateway
{
    private const string TokenDeRecusaParaTestes = "TOKEN_RECUSAR";

    public Task<ResultadoPagamento> ProcessarAsync(SolicitacaoPagamento solicitacao, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(solicitacao.TokenPagamento))
            return Task.FromResult(new ResultadoPagamento(false, null, "Token de pagamento ausente."));

        if (solicitacao.TokenPagamento.Equals(TokenDeRecusaParaTestes, StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(new ResultadoPagamento(false, null, "Pagamento recusado pela operadora."));

        return Task.FromResult(new ResultadoPagamento(true, $"FAKE-{Guid.NewGuid():N}", null));
    }
}
