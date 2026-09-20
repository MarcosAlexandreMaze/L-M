using LMStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMStore.Infrastructure.Persistence.Configurations;

public class CarrinhoConfiguration : IEntityTypeConfiguration<Carrinho>
{
    public void Configure(EntityTypeBuilder<Carrinho> builder)
    {
        builder.ToTable("Carrinhos");
        builder.HasKey(c => c.Id);

        builder.HasIndex(c => c.ClienteId).IsUnique();
        builder.HasOne<Cliente>().WithOne().HasForeignKey<Carrinho>(c => c.ClienteId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(c => c.CupomAplicadoId);

        builder.HasMany(c => c.Itens)
            .WithOne()
            .HasForeignKey(i => i.CarrinhoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.Itens).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
