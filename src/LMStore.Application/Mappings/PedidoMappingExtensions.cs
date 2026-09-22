using LMStore.Application.DTOs.Pedido;
using LMStore.Domain.Entities;

namespace LMStore.Application.Mappings;

// Diferente do CatalogoMappingExtensions, este mapeamento não precisa de nenhuma
// consulta extra (nomeMarca, nomeCategoria etc.) — ItemPedido já carrega nome/SKU/preço
// congelados desde a criação (Etapa 5), então mapear um Pedido inteiro é uma operação
// pura, sem I/O.
public static class PedidoMappingExtensions
{
    public static PedidoResponse ParaResponse(this Pedido pedido) => new(
        pedido.Id,
        pedido.Numero,
        pedido.Status.ToString(),
        [.. pedido.Itens.Select(i => i.ParaResponse())],
        pedido.Subtotal.Valor,
        pedido.DescontoCupom.Valor,
        pedido.ValorFrete.Valor,
        pedido.Total.Valor,
        pedido.FormaPagamento.ToString(),
        pedido.PagamentoAtual?.Status.ToString() ?? "Pendente",
        pedido.MotivoCancelamento,
        pedido.EnderecoEntrega.ParaResponse(),
        pedido.CriadoEm);

    public static PedidoItemResponse ParaResponse(this ItemPedido item) => new(
        item.Id, item.NomeProdutoSnapshot, item.SkuSnapshot.Codigo, item.PrecoUnitarioSnapshot.Valor, item.Quantidade, item.Subtotal.Valor);

    public static EnderecoEntregaResponse ParaResponse(this Domain.ValueObjects.EnderecoEntrega endereco) => new(
        endereco.Logradouro, endereco.Numero, endereco.Complemento, endereco.Bairro,
        endereco.Cidade, endereco.Estado, endereco.Cep.Formatado, endereco.Pais);

    public static PedidoResumoResponse ParaResumo(this Pedido pedido) => new(
        pedido.Id, pedido.Numero, pedido.Status.ToString(), pedido.Total.Valor, pedido.CriadoEm);
}
