namespace LMStore.Domain.Enums;

public enum StatusPedido
{
    AguardandoPagamento = 1,
    PagamentoAprovado = 2,
    EmPreparacao = 3,
    Enviado = 4,
    Entregue = 5,
    Cancelado = 6
}
