using LMStore.Domain.Exceptions;
using LMStore.Domain.ValueObjects;

namespace LMStore.Tests.Domain.ValueObjects;

public class CpfTests
{
    // CPF fictício, válido segundo o algoritmo de dígitos verificadores — usado
    // como exemplo em documentações e testes, não pertence a nenhuma pessoa real.
    private const string CpfValido = "529.982.247-25";

    [Fact]
    public void Deve_aceitar_cpf_valido_e_remover_a_formatacao()
    {
        var cpf = new Cpf(CpfValido);

        Assert.Equal("52998224725", cpf.Numero);
    }

    [Fact]
    public void Deve_formatar_cpf_de_volta_com_pontos_e_traco()
    {
        var cpf = new Cpf("52998224725");

        Assert.Equal(CpfValido, cpf.Formatado);
    }

    [Fact]
    public void Nao_deve_aceitar_cpf_com_digito_verificador_incorreto()
    {
        Assert.Throws<DomainException>(() => new Cpf("52998224726"));
    }

    [Fact]
    public void Nao_deve_aceitar_cpf_com_todos_os_digitos_iguais()
    {
        Assert.Throws<DomainException>(() => new Cpf("11111111111"));
    }

    [Theory]
    [InlineData("123")]
    [InlineData("")]
    [InlineData("123456789012")]
    public void Nao_deve_aceitar_cpf_com_quantidade_de_digitos_invalida(string numero)
    {
        Assert.Throws<DomainException>(() => new Cpf(numero));
    }
}
