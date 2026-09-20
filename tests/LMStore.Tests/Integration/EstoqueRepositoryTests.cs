using LMStore.Domain.Entities;
using LMStore.Domain.Enums;
using LMStore.Domain.Exceptions;
using LMStore.Domain.ValueObjects;
using LMStore.Infrastructure.Persistence;
using LMStore.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LMStore.Tests.Integration;

// Testes de integração exigem o LocalDB rodando (Etapa 6) — por isso a marcação de
// categoria: "dotnet test --filter Category!=Integration" roda só a suíte rápida em
// memória (a maioria, e a que deve rodar em qualquer máquina/CI sem infra extra).
// Limpeza/isolamento entre execuções fica para a suíte de integração definitiva da
// Etapa 14 — por ora cada seed usa nomes com Guid pra nunca colidir com o anterior.
[Trait("Category", "Integration")]
public class EstoqueRepositoryTests
{
    private const string ConnectionString =
        "Server=(localdb)\\mssqllocaldb;Database=LMStoreDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true";

    private static DbContextOptions<LMStoreDbContext> CriarOptions() =>
        new DbContextOptionsBuilder<LMStoreDbContext>().UseSqlServer(ConnectionString).Options;

    private static async Task<Guid> SemearVariacaoComEstoqueAsync(int quantidadeInicial)
    {
        await using var db = new LMStoreDbContext(CriarOptions());

        var marca = new Marca($"Marca-{Guid.NewGuid():N}");
        var categoria = new Categoria($"Categoria-{Guid.NewGuid():N}");
        var roupa = new Roupa(
            "Camiseta Teste", "Descrição de teste", marca.Id, categoria.Id, new Dinheiro(100),
            GeneroProduto.Unissex, TipoRoupa.Camiseta, "Algodão");
        var variacao = roupa.AdicionarVariacao(new Sku($"SKU-{Guid.NewGuid():N}"), null, "P", "Preta");
        var estoque = new Estoque(variacao.Id, quantidadeInicial);

        db.Marcas.Add(marca);
        db.Categorias.Add(categoria);
        db.Roupas.Add(roupa);
        db.Estoques.Add(estoque);
        await db.SaveChangesAsync();

        return variacao.Id;
    }

    [Fact]
    public async Task Reservas_concorrentes_nunca_devem_exceder_o_disponivel()
    {
        var variacaoId = await SemearVariacaoComEstoqueAsync(quantidadeInicial: 10);

        // 20 tentativas concorrentes de reservar 1 unidade cada contra um estoque de 10 —
        // cada tarefa com seu próprio DbContext (DbContext não é thread-safe).
        var tarefas = Enumerable.Range(0, 20).Select(async _ =>
        {
            await using var db = new LMStoreDbContext(CriarOptions());
            var repositorio = new EstoqueRepository(db);

            try
            {
                await repositorio.ReservarAsync(variacaoId, 1, Guid.NewGuid());
                await db.SaveChangesAsync();
                return true;
            }
            catch (DomainException)
            {
                return false;
            }
        });

        var resultados = await Task.WhenAll(tarefas);

        Assert.Equal(10, resultados.Count(sucesso => sucesso));
        Assert.Equal(10, resultados.Count(sucesso => !sucesso));

        await using var verificacao = new LMStoreDbContext(CriarOptions());
        var estoqueFinal = await verificacao.Estoques.FirstAsync(e => e.VariacaoProdutoId == variacaoId);
        Assert.Equal(0, estoqueFinal.QuantidadeDisponivel);
        Assert.Equal(10, estoqueFinal.QuantidadeReservada);
    }

    [Fact]
    public async Task Cancelar_reserva_deve_devolver_quantidade_e_registrar_movimento()
    {
        var variacaoId = await SemearVariacaoComEstoqueAsync(quantidadeInicial: 5);
        var pedidoId = Guid.NewGuid();

        await using (var db = new LMStoreDbContext(CriarOptions()))
        {
            var repositorio = new EstoqueRepository(db);
            await repositorio.ReservarAsync(variacaoId, 3, pedidoId);
            await db.SaveChangesAsync();
        }

        await using (var db = new LMStoreDbContext(CriarOptions()))
        {
            var repositorio = new EstoqueRepository(db);
            await repositorio.CancelarReservaAsync(variacaoId, 3, pedidoId);
            await db.SaveChangesAsync();
        }

        await using var verificacao = new LMStoreDbContext(CriarOptions());
        var estoqueFinal = await verificacao.Estoques.FirstAsync(e => e.VariacaoProdutoId == variacaoId);
        Assert.Equal(5, estoqueFinal.QuantidadeDisponivel);
        Assert.Equal(0, estoqueFinal.QuantidadeReservada);

        var movimentos = await verificacao.Set<MovimentoEstoque>()
            .Where(m => m.EstoqueId == estoqueFinal.Id)
            .ToListAsync();
        Assert.Contains(movimentos, m => m.Tipo == TipoMovimentoEstoque.Reserva);
        Assert.Contains(movimentos, m => m.Tipo == TipoMovimentoEstoque.LiberacaoReserva);
    }
}
