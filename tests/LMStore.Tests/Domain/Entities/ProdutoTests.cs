using LMStore.Domain.Entities;
using LMStore.Domain.Enums;
using LMStore.Domain.Exceptions;
using LMStore.Domain.ValueObjects;

namespace LMStore.Tests.Domain.Entities;

// Produto é abstrata, então os testes do comportamento compartilhado (definido em
// Produto e herdado por qualquer subtipo) passam por uma instância concreta de Roupa.
// Testes específicos de Roupa/Tenis (validações e DetalhesEspecificos) ficam em
// RoupaTests.cs e TenisTests.cs.
public class ProdutoTests
{
    private static Roupa CriarRoupa(decimal preco = 100) => new(
        "Camiseta L&M Performance", "Camiseta dry-fit para treino", Guid.NewGuid(), Guid.NewGuid(),
        new Dinheiro(preco), GeneroProduto.Unissex, TipoRoupa.Camiseta, "Poliéster");

    [Fact]
    public void Preco_vigente_deve_ser_o_preco_cheio_quando_nao_ha_promocao()
    {
        var produto = CriarRoupa(100);

        Assert.Equal(new Dinheiro(100), produto.PrecoVigente());
        Assert.False(produto.EstaEmPromocao());
    }

    [Fact]
    public void Preco_vigente_deve_ser_o_promocional_quando_aplicado()
    {
        var produto = CriarRoupa(100);

        produto.AplicarPromocao(new Dinheiro(80));

        Assert.Equal(new Dinheiro(80), produto.PrecoVigente());
        Assert.True(produto.EstaEmPromocao());
    }

    [Fact]
    public void Nao_deve_aplicar_promocao_com_preco_maior_ou_igual_ao_preco_cheio()
    {
        var produto = CriarRoupa(100);

        Assert.Throws<DomainException>(() => produto.AplicarPromocao(new Dinheiro(100)));
        Assert.Throws<DomainException>(() => produto.AplicarPromocao(new Dinheiro(150)));
    }

    [Fact]
    public void Remover_promocao_deve_voltar_ao_preco_cheio()
    {
        var produto = CriarRoupa(100);
        produto.AplicarPromocao(new Dinheiro(80));

        produto.RemoverPromocao();

        Assert.Equal(new Dinheiro(100), produto.PrecoVigente());
    }

    [Fact]
    public void Nao_deve_alterar_preco_para_valor_abaixo_da_promocao_vigente()
    {
        var produto = CriarRoupa(100);
        produto.AplicarPromocao(new Dinheiro(80));

        Assert.Throws<DomainException>(() => produto.AlterarPreco(new Dinheiro(70)));
    }

    [Fact]
    public void Deve_adicionar_variacao_com_sucesso()
    {
        var produto = CriarRoupa();

        var variacao = produto.AdicionarVariacao(new Sku("CAM-PRETA-P"), "7891234567890", "P", "Preta");

        Assert.Single(produto.Variacoes);
        Assert.True(variacao.Ativa);
    }

    [Fact]
    public void Nao_deve_adicionar_duas_variacoes_com_o_mesmo_sku()
    {
        var produto = CriarRoupa();
        produto.AdicionarVariacao(new Sku("CAM-PRETA-P"), null, "P", "Preta");

        Assert.Throws<ConflictException>(() => produto.AdicionarVariacao(new Sku("cam-preta-p"), null, "G", "Preta"));
    }

    [Fact]
    public void Deve_remover_variacao_existente()
    {
        var produto = CriarRoupa();
        var variacao = produto.AdicionarVariacao(new Sku("CAM-PRETA-P"), null, "P", "Preta");

        produto.RemoverVariacao(variacao.Id);

        Assert.Empty(produto.Variacoes);
    }

    [Fact]
    public void Remover_variacao_inexistente_deve_falhar()
    {
        var produto = CriarRoupa();

        Assert.Throws<NotFoundException>(() => produto.RemoverVariacao(Guid.NewGuid()));
    }

    [Fact]
    public void Deve_desativar_e_reativar_variacao()
    {
        var produto = CriarRoupa();
        var variacao = produto.AdicionarVariacao(new Sku("CAM-PRETA-P"), null, "P", "Preta");

        produto.DesativarVariacao(variacao.Id);
        Assert.False(variacao.Ativa);

        produto.AtivarVariacao(variacao.Id);
        Assert.True(variacao.Ativa);
    }

    [Fact]
    public void Nao_deve_desativar_produto_ja_inativo()
    {
        var produto = CriarRoupa();
        produto.Desativar();

        Assert.Throws<DomainException>(() => produto.Desativar());
    }

    [Fact]
    public void Polimorfismo_detalhes_especificos_deve_variar_por_tipo_concreto_em_tempo_de_execucao()
    {
        List<Produto> catalogo =
        [
            new Roupa("Camiseta", "Dry-fit", Guid.NewGuid(), Guid.NewGuid(), new Dinheiro(90),
                GeneroProduto.Masculino, TipoRoupa.Camiseta, "Poliéster"),
            new Tenis("Air Runner", "Tênis de corrida", Guid.NewGuid(), Guid.NewGuid(), new Dinheiro(450),
                GeneroProduto.Unissex, IndicacaoUso.Corrida, TipoPisada.Neutro)
        ];

        var detalhes = catalogo.Select(p => p.DetalhesEspecificos()).ToList();

        Assert.Equal("Camiseta · Material: Poliéster", detalhes[0]);
        Assert.Equal("Indicado para Corrida · Pisada: Neutro", detalhes[1]);
    }
}
