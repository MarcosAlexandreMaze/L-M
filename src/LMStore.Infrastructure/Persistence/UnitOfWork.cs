using LMStore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMStore.Infrastructure.Persistence;

public class UnitOfWork(LMStoreDbContext context) : IUnitOfWork
{
    public async Task<int> SalvarAsync(CancellationToken ct = default)
    {
        await CorrigirEntidadesNovasMarcadasComoModificadasAsync(ct);
        return await context.SaveChangesAsync(ct);
    }

    // Toda entidade do domínio gera seu próprio Id (Guid.CreateVersion7(), no construtor
    // — ver Entity na Etapa 2), não o banco. Isso significa que quando um filho novo é
    // anexado à coleção de um agregado JÁ RASTREADO (ex.: Usuario carregado por
    // ObterPorEmailAsync ganha um novo RefreshToken em EmitirRefreshToken), o EF não
    // consegue inferir "isso é novo" só pela chave — ela já vem preenchida com um valor
    // "real", diferente do 0/Guid.Empty que sinalizaria novidade para uma chave gerada
    // pelo banco. O EF erra para o lado seguro e marca como Modified, o que gera um
    // UPDATE contra uma linha que ainda não existe (DbUpdateConcurrencyException, 0
    // linhas afetadas). A correção genérica: perguntar ao banco se a linha realmente
    // existe: se não existir, o estado correto é Added, não Modified.
    private async Task CorrigirEntidadesNovasMarcadasComoModificadasAsync(CancellationToken ct)
    {
        context.ChangeTracker.DetectChanges();

        var candidatos = context.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).ToList();

        foreach (var entrada in candidatos)
        {
            var valoresNoBanco = await entrada.GetDatabaseValuesAsync(ct);
            if (valoresNoBanco is null)
                entrada.State = EntityState.Added;
        }
    }

    public async Task ExecutarEmTransacaoAsync(Func<Task> operacao, CancellationToken ct = default)
    {
        // CreateExecutionStrategy() é obrigatório aqui porque a conexão tem retry
        // automático habilitado (EnableRetryOnFailure, na Etapa 7) — sem envolver a
        // transação manual nesta estratégia, o EF recusa em runtime: retry automático e
        // gerenciamento manual de transação não podem coexistir sem essa coordenação
        // (se uma operação falhar no meio e for reexecutada, precisa reexecutar dentro
        // da mesma transação, não deixar a transação órfã pela metade).
        var estrategia = context.Database.CreateExecutionStrategy();

        await estrategia.ExecuteAsync(async () =>
        {
            await using var transacao = await context.Database.BeginTransactionAsync(ct);
            await operacao();
            await transacao.CommitAsync(ct);
        });
    }
}
