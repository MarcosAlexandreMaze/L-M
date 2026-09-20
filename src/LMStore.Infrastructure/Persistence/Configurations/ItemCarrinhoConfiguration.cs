using LMStore.Domain.Entities;
using LMStore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMStore.Infrastructure.Persistence.Configurations;

public class ItemCarrinhoConfiguration : IEntityTypeConfiguration<ItemCarrinho>
{
    public void Configure(EntityTypeBuilder<ItemCarrinho> builder)
    {
        builder.ToTable("ItensCarrinho");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.PrecoUnitario)
            .HasConversion(dinheiro => dinheiro.Valor, valor => new Dinheiro(valor))
            .HasColumnName("PrecoUnitario")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.Quantidade).IsRequired();

        // Igual a MovimentoEstoque: referência informativa, sem FK — um item de carrinho
        // não deveria travar a exclusão/alteração de uma variação de produto.
        builder.Property(i => i.VariacaoProdutoId).IsRequired();
    }
}
