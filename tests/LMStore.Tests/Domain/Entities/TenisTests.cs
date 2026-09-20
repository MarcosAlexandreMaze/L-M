using LMStore.Domain.Entities;
using LMStore.Domain.Enums;
using LMStore.Domain.ValueObjects;

namespace LMStore.Tests.Domain.Entities;

public class TenisTests
{
    private static Tenis CriarTenis(TipoPisada? tipoPisada = null) => new(
        "Air Runner", "Tênis de corrida", Guid.NewGuid(), Guid.NewGuid(),
        new Dinheiro(450), GeneroProduto.Unissex, IndicacaoUso.Corrida, tipoPisada);

    [Fact]
    public void Tipo_de_pisada_deve_ser_opcional()
    {
        var tenis = CriarTenis(tipoPisada: null);

        Assert.Null(tenis.TipoPisada);
    }

    [Fact]
    public void Deve_definir_tipo_de_pisada_posteriormente()
    {
        var tenis = CriarTenis();

        tenis.DefinirTipoDePisada(TipoPisada.Pronador);

        Assert.Equal(TipoPisada.Pronador, tenis.TipoPisada);
    }

    [Fact]
    public void Detalhes_especificos_sem_pisada_deve_omitir_a_informacao()
    {
        var tenis = CriarTenis(tipoPisada: null);

        Assert.Equal("Indicado para Corrida", tenis.DetalhesEspecificos());
    }

    [Fact]
    public void Detalhes_especificos_com_pisada_deve_incluir_a_informacao()
    {
        var tenis = CriarTenis(TipoPisada.Supinador);

        Assert.Equal("Indicado para Corrida · Pisada: Supinador", tenis.DetalhesEspecificos());
    }
}
