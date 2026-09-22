using LMStore.Domain.Entities;
using LMStore.Domain.Enums;
using LMStore.Domain.Exceptions;
using LMStore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMStore.Infrastructure.Persistence.Repositories;

public class EstoqueRepository(LMStoreDbContext context) : IEstoqueRepository
{
    public Task<Estoque?> ObterPorVariacaoAsync(Guid variacaoProdutoId, CancellationToken ct = default) =>
        context.Estoques.Include(e => e.Movimentos).FirstOrDefaultAsync(e => e.VariacaoProdutoId == variacaoProdutoId, ct);

    public async Task ReservarAsync(Guid variacaoProdutoId, int quantidade, Guid pedidoId, CancellationToken ct = default)
    {
        var estoqueId = await ObterIdOuFalharAsync(variacaoProdutoId, ct);

        // A checagem "tem disponível?" e a escrita "reserva" são a MESMA instrução SQL
        // (WHERE QuantidadeDisponivel >= @quantidade, no UPDATE), executada direto no
        // banco via ExecuteUpdateAsync — não passa pelo change tracking do EF nem pela
        // entidade Estoque em memória. É por isso que é seguro sob concorrência real:
        // não existe uma janela entre "ler o saldo" e "escrever a reserva" onde duas
        // requisições simultâneas possam ler o mesmo saldo e ambas passarem.
        var linhasAfetadas = await context.Estoques
            .Where(e => e.VariacaoProdutoId == variacaoProdutoId && e.QuantidadeDisponivel >= quantidade)
            .ExecuteUpdateAsync(alteracoes => alteracoes
                .SetProperty(e => e.QuantidadeDisponivel, e => e.QuantidadeDisponivel - quantidade)
                .SetProperty(e => e.QuantidadeReservada, e => e.QuantidadeReservada + quantidade), ct);

        if (linhasAfetadas == 0)
            throw new DomainException($"Estoque insuficiente para a variação '{variacaoProdutoId}'.");

        RegistrarMovimento(estoqueId, TipoMovimentoEstoque.Reserva, quantidade, "Reserva para pedido", pedidoId);
    }

    public async Task CancelarReservaAsync(Guid variacaoProdutoId, int quantidade, Guid pedidoId, CancellationToken ct = default)
    {
        var estoqueId = await ObterIdOuFalharAsync(variacaoProdutoId, ct);

        await context.Estoques
            .Where(e => e.VariacaoProdutoId == variacaoProdutoId)
            .ExecuteUpdateAsync(alteracoes => alteracoes
                .SetProperty(e => e.QuantidadeDisponivel, e => e.QuantidadeDisponivel + quantidade)
                .SetProperty(e => e.QuantidadeReservada, e => e.QuantidadeReservada - quantidade), ct);

        RegistrarMovimento(estoqueId, TipoMovimentoEstoque.LiberacaoReserva, quantidade, "Cancelamento de reserva", pedidoId);
    }

    public async Task ConfirmarSaidaAsync(Guid variacaoProdutoId, int quantidade, Guid pedidoId, CancellationToken ct = default)
    {
        var estoqueId = await ObterIdOuFalharAsync(variacaoProdutoId, ct);

        await context.Estoques
            .Where(e => e.VariacaoProdutoId == variacaoProdutoId)
            .ExecuteUpdateAsync(alteracoes => alteracoes
                .SetProperty(e => e.QuantidadeReservada, e => e.QuantidadeReservada - quantidade), ct);

        RegistrarMovimento(estoqueId, TipoMovimentoEstoque.Saida, quantidade, "Saída por envio de pedido", pedidoId);
    }

    public void Adicionar(Estoque estoque) => context.Estoques.Add(estoque);

    private async Task<Guid> ObterIdOuFalharAsync(Guid variacaoProdutoId, CancellationToken ct)
    {
        var estoqueId = await context.Estoques
            .Where(e => e.VariacaoProdutoId == variacaoProdutoId)
            .Select(e => e.Id)
            .FirstOrDefaultAsync(ct);

        if (estoqueId == Guid.Empty)
            throw new NotFoundException($"Estoque não encontrado para a variação '{variacaoProdutoId}'.");

        return estoqueId;
    }

    // MovimentoEstoque só tem construtor internal (só Estoque pode criar um, normalmente)
    // — Infrastructure consegue chamá-lo aqui graças ao InternalsVisibleTo no Domain
    // (ver LMStore.Domain.csproj), exatamente para este caminho rápido que
    // deliberadamente não passa pelo agregado Estoque.
    private void RegistrarMovimento(Guid estoqueId, TipoMovimentoEstoque tipo, int quantidade, string motivo, Guid pedidoId) =>
        context.Add(new MovimentoEstoque(estoqueId, tipo, quantidade, motivo, pedidoId));
}
