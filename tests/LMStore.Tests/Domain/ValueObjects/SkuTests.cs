using LMStore.Domain.Exceptions;
using LMStore.Domain.ValueObjects;

namespace LMStore.Tests.Domain.ValueObjects;

public class SkuTests
{
    [Fact]
    public void Deve_normalizar_para_maiusculas_e_remover_espacos_nas_pontas()
    {
        var sku = new Sku("  cam-preta-p  ");

        Assert.Equal("CAM-PRETA-P", sku.Codigo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Nao_deve_aceitar_codigo_vazio(string codigo)
    {
        Assert.Throws<DomainException>(() => new Sku(codigo));
    }

    [Fact]
    public void Nao_deve_aceitar_codigo_maior_que_cinquenta_caracteres()
    {
        var codigoMuitoLongo = new string('A', 51);

        Assert.Throws<DomainException>(() => new Sku(codigoMuitoLongo));
    }
}
