using LMStore.Domain.Entities;

namespace LMStore.Domain.Interfaces;

public interface ICategoriaRepository
{
    Task<Categoria?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    // Retorna a árvore inteira — decisão da Etapa 3: o volume de categorias de um catálogo
    // é pequeno o bastante pra montar a hierarquia em memória, sem precisar de CTE recursiva.
    Task<IReadOnlyList<Categoria>> ListarTodasAsync(CancellationToken ct = default);

    void Adicionar(Categoria categoria);
}
