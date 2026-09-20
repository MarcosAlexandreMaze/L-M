using LMStore.Domain.Exceptions;
using LMStore.Domain.ValueObjects;

namespace LMStore.Tests.Domain.ValueObjects;

public class CepTests
{
    [Fact]
    public void Deve_aceitar_cep_com_formatacao_e_remove_la()
    {
        var cep = new Cep("01310-100");

        Assert.Equal("01310100", cep.Numero);
    }

    [Fact]
    public void Deve_formatar_cep_de_volta_com_traco()
    {
        var cep = new Cep("01310100");

        Assert.Equal("01310-100", cep.Formatado);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("123456789")]
    [InlineData("")]
    public void Nao_deve_aceitar_cep_com_quantidade_de_digitos_diferente_de_oito(string numero)
    {
        Assert.Throws<DomainException>(() => new Cep(numero));
    }
}
