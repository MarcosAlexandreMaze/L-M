using LMStore.Domain.Entities;
using LMStore.Domain.Enums;
using LMStore.Domain.Exceptions;
using LMStore.Domain.ValueObjects;

namespace LMStore.Tests.Domain.Entities;

public class UsuarioTests
{
    private static Usuario CriarUsuario() =>
        new(new Email("cliente@lmstore.com"), "hash-fake", PerfilUsuario.Cliente);

    [Fact]
    public void Deve_criar_usuario_ativo_por_padrao()
    {
        var usuario = CriarUsuario();

        Assert.True(usuario.Ativo);
        Assert.Equal(PerfilUsuario.Cliente, usuario.Perfil);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Nao_deve_criar_usuario_com_hash_de_senha_vazio(string? hashInvalido)
    {
        Assert.Throws<DomainException>(() => new Usuario(new Email("x@x.com"), hashInvalido!, PerfilUsuario.Cliente));
    }

    [Fact]
    public void Deve_definir_nova_senha()
    {
        var usuario = CriarUsuario();

        usuario.DefinirSenha("novo-hash");

        Assert.Equal("novo-hash", usuario.SenhaHash);
    }

    [Fact]
    public void Nao_deve_definir_senha_vazia()
    {
        var usuario = CriarUsuario();

        Assert.Throws<DomainException>(() => usuario.DefinirSenha(""));
    }

    [Fact]
    public void Deve_desativar_usuario_ativo()
    {
        var usuario = CriarUsuario();

        usuario.Desativar();

        Assert.False(usuario.Ativo);
    }

    [Fact]
    public void Nao_deve_desativar_usuario_ja_inativo()
    {
        var usuario = CriarUsuario();
        usuario.Desativar();

        Assert.Throws<DomainException>(() => usuario.Desativar());
    }

    [Fact]
    public void Nao_deve_ativar_usuario_ja_ativo()
    {
        var usuario = CriarUsuario();

        Assert.Throws<DomainException>(() => usuario.Ativar());
    }

    [Fact]
    public void Deve_reativar_usuario_previamente_desativado()
    {
        var usuario = CriarUsuario();
        usuario.Desativar();

        usuario.Ativar();

        Assert.True(usuario.Ativo);
    }
}
