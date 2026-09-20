using LMStore.Domain.Entities;
using LMStore.Domain.Exceptions;
using LMStore.Domain.ValueObjects;

namespace LMStore.Tests.Domain.Entities;

public class CarrinhoTests
{
    [Fact]
    public void Carrinho_novo_deve_estar_vazio()
    {
        var carrinho = new Carrinho(Guid.NewGuid());

        Assert.True(carrinho.EstaVazio());
    }

    [Fact]
    public void Adicionar_item_novo_deve_incluir_na_lista()
    {
        var carrinho = new Carrinho(Guid.NewGuid());
        var variacaoId = Guid.NewGuid();

        carrinho.AdicionarItem(variacaoId, new Dinheiro(50), 2);

        var item = Assert.Single(carrinho.Itens);
        Assert.Equal(2, item.Quantidade);
    }

    [Fact]
    public void Adicionar_a_mesma_variacao_duas_vezes_deve_somar_quantidade_em_vez_de_duplicar_linha()
    {
        var carrinho = new Carrinho(Guid.NewGuid());
        var variacaoId = Guid.NewGuid();

        carrinho.AdicionarItem(variacaoId, new Dinheiro(50), 2);
        carrinho.AdicionarItem(variacaoId, new Dinheiro(50), 3);

        var item = Assert.Single(carrinho.Itens);
        Assert.Equal(5, item.Quantidade);
    }

    [Fact]
    public void Nao_deve_adicionar_item_com_quantidade_invalida()
    {
        var carrinho = new Carrinho(Guid.NewGuid());

        Assert.Throws<DomainException>(() => carrinho.AdicionarItem(Guid.NewGuid(), new Dinheiro(50), 0));
    }

    [Fact]
    public void Alterar_quantidade_para_zero_deve_falhar_indicando_uso_de_remover_item()
    {
        var carrinho = new Carrinho(Guid.NewGuid());
        var item = carrinho.AdicionarItem(Guid.NewGuid(), new Dinheiro(50), 2);

        Assert.Throws<DomainException>(() => carrinho.AlterarQuantidade(item.Id, 0));
    }

    [Fact]
    public void Remover_item_inexistente_deve_falhar()
    {
        var carrinho = new Carrinho(Guid.NewGuid());

        Assert.Throws<NotFoundException>(() => carrinho.RemoverItem(Guid.NewGuid()));
    }

    [Fact]
    public void Calcular_subtotal_deve_somar_todos_os_itens()
    {
        var carrinho = new Carrinho(Guid.NewGuid());
        carrinho.AdicionarItem(Guid.NewGuid(), new Dinheiro(50), 2); // 100
        carrinho.AdicionarItem(Guid.NewGuid(), new Dinheiro(30), 1); // 30

        Assert.Equal(new Dinheiro(130), carrinho.CalcularSubtotal());
    }

    [Fact]
    public void Aplicar_e_remover_cupom_deve_atualizar_cupom_aplicado()
    {
        var carrinho = new Carrinho(Guid.NewGuid());
        var cupomId = Guid.NewGuid();

        carrinho.AplicarCupom(cupomId);
        Assert.Equal(cupomId, carrinho.CupomAplicadoId);

        carrinho.RemoverCupom();
        Assert.Null(carrinho.CupomAplicadoId);
    }

    [Fact]
    public void Esvaziar_apos_checkout_deve_limpar_itens_e_cupom()
    {
        var carrinho = new Carrinho(Guid.NewGuid());
        carrinho.AdicionarItem(Guid.NewGuid(), new Dinheiro(50), 1);
        carrinho.AplicarCupom(Guid.NewGuid());

        carrinho.EsvaziarAposCheckout();

        Assert.True(carrinho.EstaVazio());
        Assert.Null(carrinho.CupomAplicadoId);
    }
}
