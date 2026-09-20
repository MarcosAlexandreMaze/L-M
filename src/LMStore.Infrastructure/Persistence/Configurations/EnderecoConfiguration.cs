using LMStore.Domain.Entities;
using LMStore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMStore.Infrastructure.Persistence.Configurations;

public class EnderecoConfiguration : IEntityTypeConfiguration<Endereco>
{
    public void Configure(EntityTypeBuilder<Endereco> builder)
    {
        builder.ToTable("Enderecos");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Apelido).HasMaxLength(50).IsRequired();

        builder.Property(e => e.Cep)
            .HasConversion(cep => cep.Numero, valor => new Cep(valor))
            .HasColumnName("Cep")
            .HasMaxLength(8)
            .IsRequired();

        builder.Property(e => e.Logradouro).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Numero).HasMaxLength(20).IsRequired();
        builder.Property(e => e.Complemento).HasMaxLength(200);
        builder.Property(e => e.Bairro).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Cidade).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Estado).HasMaxLength(2).IsRequired();
        builder.Property(e => e.Pais).HasMaxLength(60).IsRequired();
        builder.Property(e => e.Padrao).IsRequired();
    }
}
