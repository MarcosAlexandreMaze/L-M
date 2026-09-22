using LMStore.Application.Interfaces;
using LMStore.Domain.Entities;
using LMStore.Domain.Enums;
using LMStore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace LMStore.Infrastructure.Persistence;

// Só para desenvolvimento local (ver Program.cs) — semeia a marca e os dois primeiros
// produtos pedidos pelo usuário, passando pelos construtores reais do domínio (as
// mesmas regras e validações que valeriam para qualquer produto criado pela API mais
// tarde), e o administrador inicial (Etapa 12): como o registro público só cria
// Cliente e criar um Administrador exige JÁ estar autenticado como um, sem este seed
// não haveria absolutamente nenhum jeito de existir o primeiro admin do sistema.
public static class DbSeeder
{
    // Credenciais de DESENVOLVIMENTO apenas — nunca use um seed assim em produção.
    public const string EmailAdminPadrao = "admin@lmstore.com";
    public const string SenhaAdminPadrao = "AdminLM@2026";

    public static async Task SeedAdminAsync(LMStoreDbContext context, IPasswordHasher passwordHasher)
    {
        if (await context.Usuarios.AnyAsync(u => u.Perfil == PerfilUsuario.Administrador))
            return; // já existe pelo menos um admin — idempotente

        var email = new Email(EmailAdminPadrao);
        var usuario = new Usuario(email, passwordHasher.Hash(SenhaAdminPadrao), PerfilUsuario.Administrador);
        var administrador = new Administrador(usuario.Id, "Administrador L&M Store", "Gerente Geral");

        context.Usuarios.Add(usuario);
        context.Administradores.Add(administrador);
        await context.SaveChangesAsync();
    }

    public static async Task SeedAsync(LMStoreDbContext context)
    {
        if (await context.Marcas.AnyAsync())
            return; // já semeado — idempotente, seguro rodar toda vez que a API sobe

        var marca = new Marca("L&M");
        var categoria = new Categoria("Fitness");

        var camiseta = new Roupa(
            "Camiseta Dry-Fit L&M",
            "Camiseta esportiva em tecido dry-fit, leve e respirável, ideal para treino.",
            marca.Id, categoria.Id, new Dinheiro(89.90m), GeneroProduto.Unissex,
            TipoRoupa.Camiseta, "Dry-fit (poliéster)");
        camiseta.AdicionarVariacao(new Sku("LM-CAM-DRYFIT-P-PRETA"), null, "P", "Preta");
        camiseta.AdicionarVariacao(new Sku("LM-CAM-DRYFIT-M-PRETA"), null, "M", "Preta");
        camiseta.AdicionarVariacao(new Sku("LM-CAM-DRYFIT-G-PRETA"), null, "G", "Preta");

        var shortDryFit = new Roupa(
            "Short Dry-Fit L&M",
            "Short esportivo em tecido dry-fit, leve e respirável, ideal para treino.",
            marca.Id, categoria.Id, new Dinheiro(79.90m), GeneroProduto.Unissex,
            TipoRoupa.Shorts, "Dry-fit (poliéster)");
        shortDryFit.AdicionarVariacao(new Sku("LM-SHO-DRYFIT-P-PRETO"), null, "P", "Preto");
        shortDryFit.AdicionarVariacao(new Sku("LM-SHO-DRYFIT-M-PRETO"), null, "M", "Preto");
        shortDryFit.AdicionarVariacao(new Sku("LM-SHO-DRYFIT-G-PRETO"), null, "G", "Preto");

        context.Marcas.Add(marca);
        context.Categorias.Add(categoria);
        context.Roupas.Add(camiseta);
        context.Roupas.Add(shortDryFit);
        await context.SaveChangesAsync();

        foreach (var variacao in camiseta.Variacoes.Concat(shortDryFit.Variacoes))
            context.Estoques.Add(Estoque.Criar(variacao.Id, quantidadeInicial: 20));

        await context.SaveChangesAsync();
    }
}
