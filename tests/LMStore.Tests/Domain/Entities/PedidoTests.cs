using LMStore.Domain.Entities;
using LMStore.Domain.Enums;
using LMStore.Domain.Exceptions;
using LMStore.Domain.ValueObjects;

namespace LMStore.Tests.Domain.Entities;

public class PedidoTests
{
    private static EnderecoEntrega CriarEnderecoEntrega() => new(
        "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP", new Cep("01310-100"), "Brasil");

    private static List<ItemParaPedido> CriarItens(int quantidade = 2) =>
    [
        new(Guid.NewGuid(), "Camiseta L&M Performance", new Sku("CAM-PRETA-P"), new Dinheiro(100), quantidade)
    ];

    private static Pedido CriarPedido(IReadOnlyList<ItemParaPedido>? itens = null, Cupom? cupom = null) =>
        Pedido.CriarDeCarrinho(
            Guid.NewGuid(), itens ?? CriarItens(), CriarEnderecoEntrega(), new Dinheiro(15),
            FormaPagamento.Pix, cupom);

    [Fact]
    public void Deve_criar_pedido_aguardando_pagamento_com_totais_calculados()
    {
        var pedido = CriarPedido(); // 2 x 100 = 200 subtotal + 15 frete

        Assert.Equal(StatusPedido.AguardandoPagamento, pedido.Status);
        Assert.Equal(new Dinheiro(200), pedido.Subtotal);
        Assert.Equal(new Dinheiro(215), pedido.Total);
        Assert.NotNull(pedido.PagamentoAtual);
        Assert.Equal(StatusPagamento.Pendente, pedido.PagamentoAtual!.Status);
    }

    [Fact]
    public void Nao_deve_criar_pedido_sem_itens()
    {
        Assert.Throws<DomainException>(() => CriarPedido(itens: []));
    }

    [Fact]
    public void Numero_do_pedido_deve_ser_unico_por_construcao()
    {
        var pedido1 = CriarPedido();
        var pedido2 = CriarPedido();

        Assert.NotEqual(pedido1.Numero, pedido2.Numero);
    }

    [Fact]
    public void Deve_aplicar_desconto_de_cupom_no_total()
    {
        var cupom = new Cupom(
            "LM10", TipoDesconto.Percentual, 10, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(30),
            Dinheiro.Zero, 100);

        var pedido = CriarPedido(cupom: cupom); // subtotal 200, desconto 10% = 20, frete 15 => total 195

        Assert.Equal(new Dinheiro(20), pedido.DescontoCupom);
        Assert.Equal(new Dinheiro(195), pedido.Total);
    }

    [Fact]
    public void Confirmar_pagamento_deve_aprovar_o_pagamento_atual_e_avancar_o_status()
    {
        var pedido = CriarPedido();

        pedido.ConfirmarPagamento("tok_123");

        Assert.Equal(StatusPedido.PagamentoAprovado, pedido.Status);
        Assert.Equal(StatusPagamento.Aprovado, pedido.PagamentoAtual!.Status);
    }

    [Fact]
    public void Nao_deve_confirmar_pagamento_de_pedido_que_nao_esta_aguardando_pagamento()
    {
        var pedido = CriarPedido();
        pedido.ConfirmarPagamento("tok_123");

        Assert.Throws<DomainException>(() => pedido.ConfirmarPagamento("tok_456"));
    }

    [Fact]
    public void Recusar_pagamento_deve_manter_pedido_aguardando_pagamento()
    {
        var pedido = CriarPedido();

        pedido.RecusarPagamento();

        Assert.Equal(StatusPedido.AguardandoPagamento, pedido.Status);
        Assert.Equal(StatusPagamento.Recusado, pedido.PagamentoAtual!.Status);
    }

    [Fact]
    public void Deve_permitir_nova_tentativa_de_pagamento_apos_recusa_e_entao_confirmar()
    {
        var pedido = CriarPedido();
        pedido.RecusarPagamento();

        pedido.RegistrarNovaTentativaDePagamento();
        pedido.ConfirmarPagamento("tok_novo");

        Assert.Equal(2, pedido.Pagamentos.Count);
        Assert.Equal(StatusPedido.PagamentoAprovado, pedido.Status);
    }

    [Fact]
    public void Nao_deve_registrar_nova_tentativa_enquanto_houver_uma_pendente()
    {
        var pedido = CriarPedido(); // já nasce com um pagamento Pendente

        Assert.Throws<DomainException>(pedido.RegistrarNovaTentativaDePagamento);
    }

    [Fact]
    public void Fluxo_feliz_completo_deve_percorrer_todos_os_status_em_ordem()
    {
        var pedido = CriarPedido();

        pedido.ConfirmarPagamento("tok_123");
        Assert.Equal(StatusPedido.PagamentoAprovado, pedido.Status);

        pedido.IniciarPreparacao();
        Assert.Equal(StatusPedido.EmPreparacao, pedido.Status);

        pedido.MarcarComoEnviado("BR123456789");
        Assert.Equal(StatusPedido.Enviado, pedido.Status);

        pedido.MarcarComoEntregue();
        Assert.Equal(StatusPedido.Entregue, pedido.Status);
    }

    [Fact]
    public void Nao_deve_pular_etapas_do_fluxo()
    {
        var pedido = CriarPedido(); // AguardandoPagamento

        // Não pode ir direto para EmPreparacao sem passar por PagamentoAprovado.
        Assert.Throws<DomainException>(() => pedido.IniciarPreparacao());

        // Não pode marcar como enviado sem antes iniciar a preparação.
        Assert.Throws<DomainException>(() => pedido.MarcarComoEnviado("BR123"));

        // Não pode marcar como entregue sem antes ser enviado.
        Assert.Throws<DomainException>(() => pedido.MarcarComoEntregue());
    }

    [Fact]
    public void Marcar_como_enviado_exige_codigo_de_rastreio()
    {
        var pedido = CriarPedido();
        pedido.ConfirmarPagamento("tok_123");
        pedido.IniciarPreparacao();

        Assert.Throws<DomainException>(() => pedido.MarcarComoEnviado(""));
    }

    [Theory]
    [InlineData(StatusPedido.AguardandoPagamento)]
    [InlineData(StatusPedido.PagamentoAprovado)]
    [InlineData(StatusPedido.EmPreparacao)]
    public void Deve_permitir_cancelamento_nos_status_iniciais(StatusPedido statusAlvo)
    {
        var pedido = CriarPedido();
        AvancarPedidoAte(pedido, statusAlvo);

        pedido.Cancelar("Cliente desistiu da compra");

        Assert.Equal(StatusPedido.Cancelado, pedido.Status);
        Assert.Equal("Cliente desistiu da compra", pedido.MotivoCancelamento);
    }

    [Fact]
    public void Nao_deve_cancelar_pedido_enviado()
    {
        var pedido = CriarPedido();
        AvancarPedidoAte(pedido, StatusPedido.Enviado);

        Assert.Throws<DomainException>(() => pedido.Cancelar("Mudei de ideia"));
    }

    [Fact]
    public void Nao_deve_cancelar_pedido_entregue()
    {
        var pedido = CriarPedido();
        AvancarPedidoAte(pedido, StatusPedido.Entregue);

        Assert.Throws<DomainException>(() => pedido.Cancelar("Mudei de ideia"));
    }

    [Fact]
    public void Cancelar_sem_motivo_deve_falhar()
    {
        var pedido = CriarPedido();

        Assert.Throws<DomainException>(() => pedido.Cancelar(""));
    }

    private static void AvancarPedidoAte(Pedido pedido, StatusPedido statusAlvo)
    {
        if (statusAlvo == StatusPedido.AguardandoPagamento)
            return;

        pedido.ConfirmarPagamento("tok_123");
        if (statusAlvo == StatusPedido.PagamentoAprovado)
            return;

        pedido.IniciarPreparacao();
        if (statusAlvo == StatusPedido.EmPreparacao)
            return;

        pedido.MarcarComoEnviado("BR123456789");
        if (statusAlvo == StatusPedido.Enviado)
            return;

        pedido.MarcarComoEntregue();
    }
}
