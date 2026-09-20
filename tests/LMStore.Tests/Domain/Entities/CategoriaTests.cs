using LMStore.Domain.Entities;
using LMStore.Domain.Exceptions;

namespace LMStore.Tests.Domain.Entities;

public class CategoriaTests
{
    [Fact]
    public void Deve_gerar_slug_a_partir_do_nome_removendo_acentos_e_espacos()
    {
        var categoria = new Categoria("Camisetas Masculinas");

        Assert.Equal("camisetas-masculinas", categoria.Slug);
    }

    [Fact]
    public void Deve_remover_acentuacao_ao_gerar_slug()
    {
        var categoria = new Categoria("Tênis de Corrida");

        Assert.Equal("tenis-de-corrida", categoria.Slug);
    }

    [Fact]
    public void Renomear_deve_regenerar_o_slug()
    {
        var categoria = new Categoria("Fitness");

        categoria.Renomear("Academia & Fitness");

        Assert.Equal("academia-fitness", categoria.Slug);
    }

    [Fact]
    public void Deve_criar_categoria_raiz_sem_categoria_pai()
    {
        var categoria = new Categoria("Fitness");

        Assert.Null(categoria.CategoriaPaiId);
    }

    [Fact]
    public void Deve_criar_subcategoria_vinculada_ao_pai()
    {
        var pai = new Categoria("Fitness");

        var filha = new Categoria("Masculino", pai.Id);

        Assert.Equal(pai.Id, filha.CategoriaPaiId);
    }

    [Fact]
    public void Nao_deve_permitir_categoria_ser_pai_de_si_mesma()
    {
        var categoria = new Categoria("Fitness");

        Assert.Throws<DomainException>(() => categoria.MoverPara(categoria.Id));
    }

    [Fact]
    public void Deve_mover_categoria_para_novo_pai()
    {
        var categoria = new Categoria("Camisetas");
        var novoPai = new Categoria("Masculino");

        categoria.MoverPara(novoPai.Id);

        Assert.Equal(novoPai.Id, categoria.CategoriaPaiId);
    }

    [Fact]
    public void Nao_deve_desativar_categoria_ja_inativa()
    {
        var categoria = new Categoria("Fitness");
        categoria.Desativar();

        Assert.Throws<DomainException>(() => categoria.Desativar());
    }
}
