using LMStore.Domain.Entities;
using LMStore.Domain.Enums;
using LMStore.Domain.Exceptions;
using LMStore.Domain.ValueObjects;

namespace LMStore.Tests.Domain.Entities;

public class ClienteTests
{
    private static Cliente CriarCliente() => new(
        Guid.NewGuid(), "Maria Silva", new Cpf("529.982.247-25"), "11999999999",
        new DateOnly(1990, 5, 20));

    [Fact]
    public void Deve_criar_cliente_ativo_e_sem_enderecos()
    {
        var cliente = CriarCliente();

        Assert.Equal(StatusCliente.Ativo, cliente.Status);
        Assert.Empty(cliente.Enderecos);
    }

    [Fact]
    public void Nao_deve_criar_cliente_com_data_de_nascimento_no_futuro()
    {
        var amanha = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        Assert.Throws<DomainException>(() =>
            new Cliente(Guid.NewGuid(), "Maria Silva", new Cpf("529.982.247-25"), "11999999999", amanha));
    }

    [Fact]
    public void Primeiro_endereco_adicionado_deve_virar_padrao_automaticamente()
    {
        var cliente = CriarCliente();

        var endereco = cliente.AdicionarEndereco(
            "Casa", new Cep("01310-100"), "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");

        Assert.True(endereco.Padrao);
    }

    [Fact]
    public void Segundo_endereco_nao_deve_virar_padrao_automaticamente()
    {
        var cliente = CriarCliente();
        cliente.AdicionarEndereco("Casa", new Cep("01310-100"), "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");

        var segundo = cliente.AdicionarEndereco("Trabalho", new Cep("20040-020"), "Av. Rio Branco", "1", null, "Centro", "Rio de Janeiro", "RJ");

        Assert.False(segundo.Padrao);
    }

    [Fact]
    public void Deve_trocar_endereco_padrao_e_desmarcar_o_anterior()
    {
        var cliente = CriarCliente();
        var casa = cliente.AdicionarEndereco("Casa", new Cep("01310-100"), "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        var trabalho = cliente.AdicionarEndereco("Trabalho", new Cep("20040-020"), "Av. Rio Branco", "1", null, "Centro", "Rio de Janeiro", "RJ");

        cliente.DefinirEnderecoPadrao(trabalho.Id);

        Assert.False(casa.Padrao);
        Assert.True(trabalho.Padrao);
    }

    [Fact]
    public void Definir_endereco_padrao_com_id_inexistente_deve_falhar()
    {
        var cliente = CriarCliente();

        Assert.Throws<NotFoundException>(() => cliente.DefinirEnderecoPadrao(Guid.NewGuid()));
    }

    [Fact]
    public void Remover_endereco_padrao_deve_promover_outro_automaticamente()
    {
        var cliente = CriarCliente();
        var casa = cliente.AdicionarEndereco("Casa", new Cep("01310-100"), "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        var trabalho = cliente.AdicionarEndereco("Trabalho", new Cep("20040-020"), "Av. Rio Branco", "1", null, "Centro", "Rio de Janeiro", "RJ");

        cliente.RemoverEndereco(casa.Id);

        Assert.True(trabalho.Padrao);
        Assert.Single(cliente.Enderecos);
    }

    [Fact]
    public void Remover_unico_endereco_deve_deixar_lista_vazia_sem_erro()
    {
        var cliente = CriarCliente();
        var casa = cliente.AdicionarEndereco("Casa", new Cep("01310-100"), "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");

        cliente.RemoverEndereco(casa.Id);

        Assert.Empty(cliente.Enderecos);
    }

    [Fact]
    public void Atualizar_endereco_com_uf_invalida_deve_falhar()
    {
        var cliente = CriarCliente();
        var casa = cliente.AdicionarEndereco("Casa", new Cep("01310-100"), "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");

        Assert.Throws<DomainException>(() => cliente.AtualizarEndereco(
            casa.Id, "Casa", new Cep("01310-100"), "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "XX", "Brasil"));
    }

    [Fact]
    public void Atualizar_endereco_deve_alterar_os_campos()
    {
        var cliente = CriarCliente();
        var casa = cliente.AdicionarEndereco("Casa", new Cep("01310-100"), "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");

        cliente.AtualizarEndereco(
            casa.Id, "Apê novo", new Cep("04538-132"), "Av. Faria Lima", "2000", "Ap. 12", "Itaim Bibi", "São Paulo", "SP", "Brasil");

        Assert.Equal("Apê novo", casa.Apelido);
        Assert.Equal("Av. Faria Lima", casa.Logradouro);
        Assert.Equal("Ap. 12", casa.Complemento);
    }

    [Fact]
    public void Nao_deve_desativar_cliente_ja_inativo()
    {
        var cliente = CriarCliente();
        cliente.Desativar();

        Assert.Throws<DomainException>(() => cliente.Desativar());
    }

    [Fact]
    public void Clientes_com_ids_diferentes_nao_devem_ser_iguais_mesmo_com_dados_iguais()
    {
        var cliente1 = CriarCliente();
        var cliente2 = CriarCliente();

        Assert.NotEqual(cliente1, cliente2);
        Assert.False(cliente1 == cliente2);
    }

    [Fact]
    public void Mesma_instancia_deve_ser_igual_a_si_mesma()
    {
        var cliente = CriarCliente();
        var mesmaReferencia = cliente;

        Assert.Equal(cliente, mesmaReferencia);
        Assert.True(cliente == mesmaReferencia);
    }
}
