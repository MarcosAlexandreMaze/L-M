using LMStore.Domain.Common;
using LMStore.Domain.Enums;
using LMStore.Domain.Exceptions;

namespace LMStore.Domain.Entities;

public class Estoque : Entity
{
    private readonly List<MovimentoEstoque> _movimentos = [];

    public Guid VariacaoProdutoId { get; }
    public int QuantidadeDisponivel { get; private set; }
    public int QuantidadeReservada { get; private set; }
    public IReadOnlyList<MovimentoEstoque> Movimentos => _movimentos.AsReadOnly();

    // O parâmetro se chama "quantidadeDisponivel" (e não "quantidadeInicial") de propósito:
    // o EF Core só consegue vincular automaticamente um parâmetro de construtor a uma
    // propriedade com o mesmo nome (fazendo o binding direto na materialização, sem
    // precisar de um setter público) — ver Etapa 6 para o contexto completo.
    public Estoque(Guid variacaoProdutoId, int quantidadeDisponivel = 0)
    {
        if (quantidadeDisponivel < 0)
            throw new DomainException("A quantidade inicial não pode ser negativa.");

        VariacaoProdutoId = variacaoProdutoId;
        QuantidadeDisponivel = quantidadeDisponivel;
        QuantidadeReservada = 0;

        if (quantidadeDisponivel > 0)
            RegistrarMovimento(TipoMovimentoEstoque.Entrada, quantidadeDisponivel, "Estoque inicial", null);
    }

    public void Reservar(int quantidade, Guid pedidoId)
    {
        ExigirQuantidadePositiva(quantidade);

        if (quantidade > QuantidadeDisponivel)
            throw new DomainException(
                $"Estoque insuficiente: disponível {QuantidadeDisponivel}, solicitado {quantidade}.");

        QuantidadeDisponivel -= quantidade;
        QuantidadeReservada += quantidade;
        RegistrarMovimento(TipoMovimentoEstoque.Reserva, quantidade, "Reserva para pedido", pedidoId);
    }

    public void CancelarReserva(int quantidade, Guid pedidoId)
    {
        ExigirQuantidadePositiva(quantidade);

        if (quantidade > QuantidadeReservada)
            throw new DomainException(
                $"Não é possível liberar {quantidade} unidade(s): apenas {QuantidadeReservada} estão reservadas.");

        QuantidadeReservada -= quantidade;
        QuantidadeDisponivel += quantidade;
        RegistrarMovimento(TipoMovimentoEstoque.LiberacaoReserva, quantidade, "Cancelamento de reserva", pedidoId);
    }

    public void ConfirmarSaida(int quantidade, Guid pedidoId)
    {
        ExigirQuantidadePositiva(quantidade);

        if (quantidade > QuantidadeReservada)
            throw new DomainException(
                $"Não é possível confirmar saída de {quantidade} unidade(s): apenas {QuantidadeReservada} estão reservadas.");

        QuantidadeReservada -= quantidade;
        RegistrarMovimento(TipoMovimentoEstoque.Saida, quantidade, "Saída por envio de pedido", pedidoId);
    }

    public void Repor(int quantidade, string motivo)
    {
        ExigirQuantidadePositiva(quantidade);
        ExigirMotivoPreenchido(motivo);

        QuantidadeDisponivel += quantidade;
        RegistrarMovimento(TipoMovimentoEstoque.Entrada, quantidade, motivo, null);
    }

    public void AjustarPara(int novaQuantidadeDisponivel, string motivo)
    {
        if (novaQuantidadeDisponivel < 0)
            throw new DomainException("A quantidade ajustada não pode ser negativa.");

        ExigirMotivoPreenchido(motivo);

        var diferenca = novaQuantidadeDisponivel - QuantidadeDisponivel;

        if (diferenca == 0)
            throw new DomainException("A quantidade informada é igual à quantidade atual — nenhum ajuste necessário.");

        QuantidadeDisponivel = novaQuantidadeDisponivel;
        RegistrarMovimento(TipoMovimentoEstoque.Ajuste, diferenca, motivo, null);
    }

    private static void ExigirQuantidadePositiva(int quantidade)
    {
        if (quantidade <= 0)
            throw new DomainException("A quantidade deve ser maior que zero.");
    }

    private static void ExigirMotivoPreenchido(string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new DomainException("É necessário informar um motivo para esta operação de estoque.");
    }

    private void RegistrarMovimento(TipoMovimentoEstoque tipo, int quantidade, string motivo, Guid? pedidoId) =>
        _movimentos.Add(new MovimentoEstoque(Id, tipo, quantidade, motivo, pedidoId));
}
