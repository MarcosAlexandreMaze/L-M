using LMStore.Domain.Exceptions;
using LMStore.Domain.ValueObjects;

namespace LMStore.Tests.Domain.ValueObjects;

public class DinheiroTests
{
    [Fact]
    public void Deve_arredondar_valor_para_duas_casas_decimais()
    {
        var dinheiro = new Dinheiro(19.999m);

        Assert.Equal(20.00m, dinheiro.Valor);
    }

    [Fact]
    public void Nao_deve_permitir_valor_negativo()
    {
        Assert.Throws<DomainException>(() => new Dinheiro(-1));
    }

    [Fact]
    public void Deve_somar_dois_valores()
    {
        var total = new Dinheiro(100).Somar(new Dinheiro(50));

        Assert.Equal(150m, total.Valor);
    }

    [Fact]
    public void Subtracao_nao_deve_gerar_valor_negativo()
    {
        var resultado = new Dinheiro(10).Subtrair(new Dinheiro(50));

        Assert.Equal(0m, resultado.Valor);
    }

    [Fact]
    public void Deve_multiplicar_pela_quantidade()
    {
        var total = new Dinheiro(25).MultiplicarPor(3);

        Assert.Equal(75m, total.Valor);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Nao_deve_aceitar_percentual_de_desconto_fora_de_0_a_100(decimal percentual)
    {
        Assert.Throws<DomainException>(() => new Dinheiro(100).AplicarPercentualDeDesconto(percentual));
    }

    [Fact]
    public void Deve_aplicar_percentual_de_desconto_corretamente()
    {
        var comDesconto = new Dinheiro(200).AplicarPercentualDeDesconto(10);

        Assert.Equal(180m, comDesconto.Valor);
    }

    [Fact]
    public void Dois_dinheiros_com_mesmo_valor_devem_ser_iguais()
    {
        Assert.Equal(new Dinheiro(50), new Dinheiro(50));
    }

    [Fact]
    public void Operadores_de_comparacao_devem_respeitar_o_valor()
    {
        Assert.True(new Dinheiro(100) > new Dinheiro(50));
        Assert.True(new Dinheiro(50) < new Dinheiro(100));
        Assert.True(new Dinheiro(50) >= new Dinheiro(50));
        Assert.True(new Dinheiro(50) <= new Dinheiro(50));
    }
}
