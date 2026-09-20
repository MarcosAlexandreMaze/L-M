using LMStore.Domain.Entities;
using LMStore.Domain.Enums;
using LMStore.Domain.Exceptions;
using LMStore.Domain.ValueObjects;

namespace LMStore.Tests.Domain.Entities;

public class CupomTests
{
    private static Cupom CriarCupomPercentual(decimal percentual = 10, int limiteUso = 100) => new(
        "LM10", TipoDesconto.Percentual, percentual, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(30),
        new Dinheiro(100), limiteUso);

    private static Cupom CriarCupomFixo(decimal valor = 20, int limiteUso = 100) => new(
        "LM20OFF", TipoDesconto.Fixo, valor, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(30),
        new Dinheiro(100), limiteUso);

    [Fact]
    public void Nao_deve_criar_cupom_com_data_fim_anterior_ou_igual_a_data_inicio()
    {
        var agora = DateTime.UtcNow;

        Assert.Throws<DomainException>(() =>
            new Cupom("X", TipoDesconto.Fixo, 10, agora, agora, new Dinheiro(0), 10));
    }

    [Fact]
    public void Nao_deve_criar_cupom_percentual_acima_de_100()
    {
        Assert.Throws<DomainException>(() => CriarCupomPercentual(101));
    }

    [Fact]
    public void Deve_calcular_desconto_percentual_corretamente()
    {
        var cupom = CriarCupomPercentual(10);

        var desconto = cupom.CalcularDesconto(new Dinheiro(200));

        Assert.Equal(new Dinheiro(20), desconto);
    }

    [Fact]
    public void Deve_calcular_desconto_fixo_corretamente()
    {
        var cupom = CriarCupomFixo(20);

        var desconto = cupom.CalcularDesconto(new Dinheiro(200));

        Assert.Equal(new Dinheiro(20), desconto);
    }

    [Fact]
    public void Desconto_fixo_nao_deve_ultrapassar_o_subtotal()
    {
        var cupom = CriarCupomFixo(500);

        var desconto = cupom.CalcularDesconto(new Dinheiro(100));

        Assert.Equal(new Dinheiro(100), desconto);
    }

    [Fact]
    public void Cupom_expirado_nao_deve_ser_valido()
    {
        var cupom = new Cupom(
            "VENCIDO", TipoDesconto.Fixo, 10, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(-1),
            new Dinheiro(0), 10);

        var (valido, motivo) = cupom.EstaValidoPara(new Dinheiro(100), DateTime.UtcNow);

        Assert.False(valido);
        Assert.Contains("expirado", motivo);
    }

    [Fact]
    public void Cupom_ainda_nao_iniciado_nao_deve_ser_valido()
    {
        var cupom = new Cupom(
            "FUTURO", TipoDesconto.Fixo, 10, DateTime.UtcNow.AddDays(5), DateTime.UtcNow.AddDays(10),
            new Dinheiro(0), 10);

        var (valido, _) = cupom.EstaValidoPara(new Dinheiro(100), DateTime.UtcNow);

        Assert.False(valido);
    }

    [Fact]
    public void Cupom_inativo_nao_deve_ser_valido()
    {
        var cupom = CriarCupomFixo();
        cupom.Desativar();

        var (valido, motivo) = cupom.EstaValidoPara(new Dinheiro(200), DateTime.UtcNow);

        Assert.False(valido);
        Assert.Contains("inativo", motivo);
    }

    [Fact]
    public void Compra_abaixo_do_valor_minimo_nao_deve_ser_valida()
    {
        var cupom = CriarCupomFixo(); // valorMinimoCompra = 100

        var (valido, motivo) = cupom.EstaValidoPara(new Dinheiro(50), DateTime.UtcNow);

        Assert.False(valido);
        Assert.Contains("mínimo", motivo);
    }

    [Fact]
    public void Cupom_no_limite_de_uso_nao_deve_ser_valido()
    {
        var cupom = CriarCupomFixo(limiteUso: 1);
        cupom.RegistrarUso();

        var (valido, motivo) = cupom.EstaValidoPara(new Dinheiro(200), DateTime.UtcNow);

        Assert.False(valido);
        Assert.Contains("limite", motivo);
    }

    [Fact]
    public void Nao_deve_registrar_uso_alem_do_limite()
    {
        var cupom = CriarCupomFixo(limiteUso: 1);
        cupom.RegistrarUso();

        Assert.Throws<DomainException>(cupom.RegistrarUso);
    }

    [Fact]
    public void Calcular_desconto_de_cupom_invalido_deve_lancar_excecao_com_o_motivo()
    {
        var cupom = CriarCupomFixo();
        cupom.Desativar();

        var excecao = Assert.Throws<DomainException>(() => cupom.CalcularDesconto(new Dinheiro(200)));
        Assert.Contains("inativo", excecao.Message);
    }
}
