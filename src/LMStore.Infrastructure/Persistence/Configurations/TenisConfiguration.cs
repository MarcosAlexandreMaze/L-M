using LMStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMStore.Infrastructure.Persistence.Configurations;

public class TenisConfiguration : IEntityTypeConfiguration<Tenis>
{
    public void Configure(EntityTypeBuilder<Tenis> builder)
    {
        builder.Property(t => t.TipoPisada).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.IndicacaoUso).HasConversion<string>().HasMaxLength(20).IsRequired();
    }
}
