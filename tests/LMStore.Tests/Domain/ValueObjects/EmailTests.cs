using LMStore.Domain.Exceptions;
using LMStore.Domain.ValueObjects;

namespace LMStore.Tests.Domain.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Deve_normalizar_para_minusculas_e_remover_espacos_nas_pontas()
    {
        var email = new Email("  Cliente@LMStore.com.br  ");

        Assert.Equal("cliente@lmstore.com.br", email.Endereco);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("sem-arroba.com")]
    [InlineData("sem-dominio@")]
    [InlineData("@sem-usuario.com")]
    [InlineData("com espaco@lmstore.com")]
    public void Nao_deve_aceitar_formatos_invalidos(string enderecoInvalido)
    {
        Assert.Throws<DomainException>(() => new Email(enderecoInvalido));
    }

    [Fact]
    public void Dois_emails_equivalentes_apos_normalizacao_devem_ser_iguais()
    {
        Assert.Equal(new Email("Cliente@LMStore.com"), new Email("cliente@lmstore.com"));
    }
}
