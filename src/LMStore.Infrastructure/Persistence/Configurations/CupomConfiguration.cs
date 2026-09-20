using LMStore.Domain.Entities;
using LMStore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMStore.Infrastructure.Persistence.Configurations;

public class CupomConfiguration : IEntityTypeConfiguration<Cupom>
{
    public void Configure(EntityTypeBuilder<Cupom> builder)
    {
        builder.ToTable("Cupons");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Codigo).HasMaxLength(30).IsRequired();
        builder.HasIndex(c => c.Codigo).IsUnique();

        builder.Property(c => c.TipoDesconto).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(c => c.Valor).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(c => c.DataInicio).IsRequired();
        builder.Property(c => c.DataFim).IsRequired();

        builder.Property(c => c.ValorMinimoCompra)
            .HasConversion(dinheiro => dinheiro.Valor, valor => new Dinheiro(valor))
            .HasColumnName("ValorMinimoCompra")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(c => c.LimiteUso).IsRequired();
        builder.Property(c => c.QuantidadeUtilizada).IsRequired();
        builder.Property(c => c.Ativo).IsRequired();
    }
}
