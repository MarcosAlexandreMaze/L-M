using LMStore.Domain.Entities;
using LMStore.Domain.Enums;
using LMStore.Domain.Exceptions;

namespace LMStore.Tests.Domain.Entities;

public class EstoqueTests
{
    [Fact]
    public void Construtor_deve_criar_estoque_sem_nenhum_movimento_mesmo_com_quantidade_positiva()
    {
        // Regressão de um bug real (Etapa 12): o EF Core reexecuta o construtor toda
        // vez que materializa um Estoque já existente vindo do banco. Se o construtor
        // registrasse um movimento aqui, cada LEITURA de um estoque existente geraria
        // um MovimentoEstoque "fantasma" a mais em memória. Por isso o construtor tem
        // que ficar puro — quem registra o movimento de entrada é Estoque.Criar().
        var estoque = new Estoque(Guid.NewGuid(), quantidadeDisponivel: 50);

        Assert.Equal(50, estoque.QuantidadeDisponivel);
        Assert.Empty(estoque.Movimentos);
    }

    [Fact]
    public void Estoque_zerado_por_padrao_nao_deve_ter_movimentos()
    {
        var estoque = new Estoque(Guid.NewGuid());

        Assert.Equal(0, estoque.QuantidadeDisponivel);
        Assert.Empty(estoque.Movimentos);
    }

    [Fact]
    public void Criar_com_quantidade_inicial_deve_registrar_movimento_de_entrada()
    {
        var estoque = Estoque.Criar(Guid.NewGuid(), quantidadeInicial: 50);

        Assert.Equal(50, estoque.QuantidadeDisponivel);
        var movimento = Assert.Single(estoque.Movimentos);
        Assert.Equal(TipoMovimentoEstoque.Entrada, movimento.Tipo);
        Assert.Equal(50, movimento.Quantidade);
    }

    [Fact]
    public void Criar_com_quantidade_zero_nao_deve_registrar_movimento()
    {
        var estoque = Estoque.Criar(Guid.NewGuid());

        Assert.Empty(estoque.Movimentos);
    }

    [Fact]
    public void Nao_deve_criar_estoque_com_quantidade_inicial_negativa()
    {
        Assert.Throws<DomainException>(() => new Estoque(Guid.NewGuid(), quantidadeDisponivel: -1));
    }

    [Fact]
    public void Reservar_deve_mover_quantidade_de_disponivel_para_reservada()
    {
        var estoque = Estoque.Criar(Guid.NewGuid(), 10);

        estoque.Reservar(4, Guid.NewGuid());

        Assert.Equal(6, estoque.QuantidadeDisponivel);
        Assert.Equal(4, estoque.QuantidadeReservada);
    }

    [Fact]
    public void Nao_deve_reservar_mais_do_que_o_disponivel()
    {
        var estoque = Estoque.Criar(Guid.NewGuid(), 5);

        Assert.Throws<DomainException>(() => estoque.Reservar(6, Guid.NewGuid()));
    }

    [Fact]
    public void Reserva_que_falha_nao_deve_alterar_o_estado_do_estoque()
    {
        // Garante que a validação acontece ANTES de qualquer mutação — se a ordem
        // fosse invertida, uma reserva rejeitada ainda deixaria o estoque corrompido.
        var estoque = Estoque.Criar(Guid.NewGuid(), 5);

        Assert.Throws<DomainException>(() => estoque.Reservar(10, Guid.NewGuid()));

        Assert.Equal(5, estoque.QuantidadeDisponivel);
        Assert.Equal(0, estoque.QuantidadeReservada);
        Assert.Single(estoque.Movimentos); // só a entrada inicial de Criar(), nenhum movimento da reserva rejeitada
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Nao_deve_reservar_quantidade_zero_ou_negativa(int quantidadeInvalida)
    {
        var estoque = Estoque.Criar(Guid.NewGuid(), 10);

        Assert.Throws<DomainException>(() => estoque.Reservar(quantidadeInvalida, Guid.NewGuid()));
    }

    [Fact]
    public void Cancelar_reserva_deve_devolver_quantidade_ao_disponivel()
    {
        var estoque = Estoque.Criar(Guid.NewGuid(), 10);
        var pedidoId = Guid.NewGuid();
        estoque.Reservar(4, pedidoId);

        estoque.CancelarReserva(4, pedidoId);

        Assert.Equal(10, estoque.QuantidadeDisponivel);
        Assert.Equal(0, estoque.QuantidadeReservada);
    }

    [Fact]
    public void Nao_deve_cancelar_reserva_maior_do_que_a_reservada()
    {
        var estoque = Estoque.Criar(Guid.NewGuid(), 10);
        var pedidoId = Guid.NewGuid();
        estoque.Reservar(4, pedidoId);

        Assert.Throws<DomainException>(() => estoque.CancelarReserva(5, pedidoId));
    }

    [Fact]
    public void Confirmar_saida_deve_reduzir_apenas_a_quantidade_reservada()
    {
        var estoque = Estoque.Criar(Guid.NewGuid(), 10);
        var pedidoId = Guid.NewGuid();
        estoque.Reservar(4, pedidoId);

        estoque.ConfirmarSaida(4, pedidoId);

        Assert.Equal(6, estoque.QuantidadeDisponivel);
        Assert.Equal(0, estoque.QuantidadeReservada);
    }

    [Fact]
    public void Nao_deve_confirmar_saida_maior_do_que_a_reservada()
    {
        var estoque = Estoque.Criar(Guid.NewGuid(), 10);

        Assert.Throws<DomainException>(() => estoque.ConfirmarSaida(1, Guid.NewGuid()));
    }

    [Fact]
    public void Repor_deve_aumentar_a_quantidade_disponivel()
    {
        var estoque = Estoque.Criar(Guid.NewGuid(), 10);

        estoque.Repor(20, "Novo lote recebido do fornecedor");

        Assert.Equal(30, estoque.QuantidadeDisponivel);
    }

    [Fact]
    public void Nao_deve_repor_sem_informar_motivo()
    {
        var estoque = Estoque.Criar(Guid.NewGuid(), 10);

        Assert.Throws<DomainException>(() => estoque.Repor(10, ""));

        // A falha de validação não pode ter alterado o estado.
        Assert.Equal(10, estoque.QuantidadeDisponivel);
    }

    [Fact]
    public void Ajustar_para_deve_aceitar_correcao_para_cima_ou_para_baixo()
    {
        var estoque = Estoque.Criar(Guid.NewGuid(), 10);

        estoque.AjustarPara(7, "Contagem de inventário");

        Assert.Equal(7, estoque.QuantidadeDisponivel);
        var movimento = Assert.Single(estoque.Movimentos, m => m.Tipo == TipoMovimentoEstoque.Ajuste);
        Assert.Equal(-3, movimento.Quantidade);
    }

    [Fact]
    public void Ajustar_para_o_mesmo_valor_atual_deve_falhar()
    {
        var estoque = Estoque.Criar(Guid.NewGuid(), 10);

        Assert.Throws<DomainException>(() => estoque.AjustarPara(10, "Sem mudança"));
    }
}
