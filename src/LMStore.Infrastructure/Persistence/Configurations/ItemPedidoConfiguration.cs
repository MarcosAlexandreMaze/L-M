using LMStore.Domain.Entities;
using LMStore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMStore.Infrastructure.Persistence.Configurations;

public class ItemPedidoConfiguration : IEntityTypeConfiguration<ItemPedido>
{
    public void Configure(EntityTypeBuilder<ItemPedido> builder)
    {
        builder.ToTable("ItensPedido");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.NomeProdutoSnapshot).HasColumnName("NomeProduto").HasMaxLength(150).IsRequired();

        builder.Property(i => i.SkuSnapshot)
            .HasConversion(sku => sku.Codigo, valor => new Sku(valor))
            .HasColumnName("Sku")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.PrecoUnitarioSnapshot)
            .HasConversion(dinheiro => dinheiro.Valor, valor => new Dinheiro(valor))
            .HasColumnName("PrecoUnitario")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.Quantidade).IsRequired();

        // Igual a MovimentoEstoque e ItemCarrinho: sem FK para VariacaoProduto. O item
        // já carrega tudo que precisa (nome, SKU, preço) — o produto original pode ser
        // editado, desativado ou até removido sem que isso afete um pedido já feito.
        builder.Property(i => i.VariacaoProdutoId).IsRequired();

        builder.Ignore(i => i.Subtotal); // calculada em memória (PrecoUnitarioSnapshot x Quantidade), não persistida
    }
}
