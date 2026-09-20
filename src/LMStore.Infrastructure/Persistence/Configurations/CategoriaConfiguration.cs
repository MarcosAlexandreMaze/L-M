using LMStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMStore.Infrastructure.Persistence.Configurations;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categorias");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome).HasMaxLength(100).IsRequired();

        builder.Property(c => c.Slug).HasMaxLength(120).IsRequired();
        builder.HasIndex(c => c.Slug).IsUnique();

        // Auto-relacionamento: SQL Server não permite CASCADE em FK que aponta para a
        // própria tabela (caminhos de cascata ambíguos) — Restrict é obrigatório aqui.
        builder.HasOne<Categoria>()
            .WithMany()
            .HasForeignKey(c => c.CategoriaPaiId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
