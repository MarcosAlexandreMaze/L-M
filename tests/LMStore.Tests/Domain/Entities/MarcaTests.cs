using LMStore.Domain.Entities;
using LMStore.Domain.Exceptions;

namespace LMStore.Tests.Domain.Entities;

public class MarcaTests
{
    [Fact]
    public void Deve_criar_marca_ativa()
    {
        var marca = new Marca("Nike");

        Assert.True(marca.Ativa);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Nao_deve_criar_marca_sem_nome(string? nomeInvalido)
    {
        Assert.Throws<DomainException>(() => new Marca(nomeInvalido!));
    }

    [Fact]
    public void Deve_renomear_marca()
    {
        var marca = new Marca("Nkie");

        marca.Renomear("Nike");

        Assert.Equal("Nike", marca.Nome);
    }

    [Fact]
    public void Nao_deve_ativar_marca_ja_ativa()
    {
        var marca = new Marca("Nike");

        Assert.Throws<DomainException>(() => marca.Ativar());
    }
}
