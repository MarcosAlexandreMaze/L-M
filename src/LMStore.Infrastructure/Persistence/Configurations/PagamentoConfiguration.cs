using LMStore.Domain.Entities;
using LMStore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMStore.Infrastructure.Persistence.Configurations;

public class PagamentoConfiguration : IEntityTypeConfiguration<Pagamento>
{
    public void Configure(EntityTypeBuilder<Pagamento> builder)
    {
        builder.ToTable("Pagamentos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.FormaPagamento).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Property(p => p.ValorPago)
            .HasConversion(dinheiro => dinheiro.Valor, valor => new Dinheiro(valor))
            .HasColumnName("ValorPago")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.DataProcessamento);
        builder.Property(p => p.TokenTransacaoExterna).HasMaxLength(200);
    }
}
