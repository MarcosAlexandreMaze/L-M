using LMStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMStore.Infrastructure.Persistence.Configurations;

// Sem ToTable() aqui: em TPH, os subtipos compartilham a tabela da base (Produtos).
// Esta classe só acrescenta o mapeamento dos campos exclusivos de Roupa.
public class RoupaConfiguration : IEntityTypeConfiguration<Roupa>
{
    public void Configure(EntityTypeBuilder<Roupa> builder)
    {
        builder.Property(r => r.TipoRoupa).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.Material).HasMaxLength(100).IsRequired();
    }
}
