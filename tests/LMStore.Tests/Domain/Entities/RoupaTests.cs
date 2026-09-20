using LMStore.Domain.Entities;
using LMStore.Domain.Enums;
using LMStore.Domain.Exceptions;
using LMStore.Domain.ValueObjects;

namespace LMStore.Tests.Domain.Entities;

public class RoupaTests
{
    private static Roupa CriarRoupa(string material = "Poliéster") => new(
        "Camiseta L&M Performance", "Camiseta dry-fit para treino", Guid.NewGuid(), Guid.NewGuid(),
        new Dinheiro(100), GeneroProduto.Unissex, TipoRoupa.Camiseta, material);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Nao_deve_criar_roupa_sem_material(string? materialInvalido)
    {
        Assert.Throws<DomainException>(() => CriarRoupa(materialInvalido!));
    }

    [Fact]
    public void Deve_atualizar_material()
    {
        var roupa = CriarRoupa();

        roupa.AtualizarMaterial("Algodão");

        Assert.Equal("Algodão", roupa.Material);
    }

    [Fact]
    public void Detalhes_especificos_deve_combinar_tipo_e_material()
    {
        var roupa = CriarRoupa("Algodão");

        Assert.Equal("Camiseta · Material: Algodão", roupa.DetalhesEspecificos());
    }
}
