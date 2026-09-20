using LMStore.Domain.Entities;

namespace LMStore.Domain.Interfaces;

public interface IEstoqueRepository
{
    Task<Estoque?> ObterPorVariacaoAsync(Guid variacaoProdutoId, CancellationToken ct = default);

    // As três operações abaixo são deliberadamente atômicas (UPDATE condicional único,
    // não "carregar agregado, mutar em memória, salvar") — ver EstoqueRepository na
    // Infrastructure para a técnica e a justificativa completa (Etapa 7).
    Task ReservarAsync(Guid variacaoProdutoId, int quantidade, Guid pedidoId, CancellationToken ct = default);
    Task CancelarReservaAsync(Guid variacaoProdutoId, int quantidade, Guid pedidoId, CancellationToken ct = default);
    Task ConfirmarSaidaAsync(Guid variacaoProdutoId, int quantidade, Guid pedidoId, CancellationToken ct = default);

    void Adicionar(Estoque estoque);
}
