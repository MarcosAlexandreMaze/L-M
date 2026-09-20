using LMStore.Domain.Entities;
using LMStore.Domain.Enums;
using LMStore.Domain.ValueObjects;
using LMStore.Infrastructure.Persistence;
using LMStore.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LMStore.Tests.Integration;

[Trait("Category", "Integration")]
public class UnitOfWorkTests
{
    private const string ConnectionString =
        "Server=(localdb)\\mssqllocaldb;Database=LMStoreDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true";

    private static DbContextOptions<LMStoreDbContext> CriarOptions() =>
        new DbContextOptionsBuilder<LMStoreDbContext>().UseSqlServer(ConnectionString).Options;

    [Fact]
    public async Task Adicionar_entidade_filha_nova_a_agregado_ja_carregado_deve_inserir_nao_atualizar()
    {
        // Reproduz o bug real encontrado testando o login manualmente: como toda
        // entidade gera seu próprio Guid no construtor (Etapa 2), o EF não consegue
        // inferir "isso é novo" só pela chave quando um filho é anexado à coleção de
        // um agregado JÁ CARREGADO por uma query — ele marca como Modified em vez de
        // Added, e o SaveChanges falha com DbUpdateConcurrencyException (UPDATE contra
        // uma linha que não existe). UnitOfWork.SalvarAsync corrige isso perguntando ao
        // banco se a linha realmente existe antes de aceitar o estado Modified.
        var options = CriarOptions();
        var email = new Email($"regressao-{Guid.NewGuid():N}@lmstore.com");

        Guid usuarioId;
        await using (var db = new LMStoreDbContext(options))
        {
            var usuario = new Usuario(email, "hash-fake", PerfilUsuario.Cliente);
            usuario.EmitirRefreshToken("hash-token-1", TimeSpan.FromDays(7));

            db.Usuarios.Add(usuario);
            await db.SaveChangesAsync();
            usuarioId = usuario.Id;
        }

        // Novo DbContext, simulando uma segunda requisição HTTP (ex.: um segundo login).
        await using (var db = new LMStoreDbContext(options))
        {
            var repositorio = new UsuarioRepository(db);
            var unitOfWork = new UnitOfWork(db);

            var usuario = await repositorio.ObterPorEmailAsync(email);
            usuario!.EmitirRefreshToken("hash-token-2", TimeSpan.FromDays(7));

            await unitOfWork.SalvarAsync(); // não deve lançar DbUpdateConcurrencyException
        }

        await using var verificacao = new LMStoreDbContext(options);
        var tokens = await verificacao.Set<RefreshToken>()
            .Where(t => t.UsuarioId == usuarioId)
            .ToListAsync();

        Assert.Equal(2, tokens.Count);
    }
}
