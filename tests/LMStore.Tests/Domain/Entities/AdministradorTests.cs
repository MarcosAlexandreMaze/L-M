using LMStore.Domain.Entities;
using LMStore.Domain.Exceptions;

namespace LMStore.Tests.Domain.Entities;

public class AdministradorTests
{
    [Fact]
    public void Deve_criar_administrador_sem_cargo_informado()
    {
        var administrador = new Administrador(Guid.NewGuid(), "João Souza");

        Assert.Null(administrador.Cargo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Nao_deve_criar_administrador_sem_nome(string? nomeInvalido)
    {
        Assert.Throws<DomainException>(() => new Administrador(Guid.NewGuid(), nomeInvalido!));
    }

    [Fact]
    public void Deve_atualizar_nome_e_cargo()
    {
        var administrador = new Administrador(Guid.NewGuid(), "João Souza");

        administrador.AtualizarDados("João P. Souza", "Gerente de Estoque");

        Assert.Equal("João P. Souza", administrador.Nome);
        Assert.Equal("Gerente de Estoque", administrador.Cargo);
    }
}
