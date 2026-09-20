using LMStore.Domain.Entities;
using LMStore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMStore.Infrastructure.Persistence.Configurations;

public class VariacaoProdutoConfiguration : IEntityTypeConfiguration<VariacaoProduto>
{
    public void Configure(EntityTypeBuilder<VariacaoProduto> builder)
    {
        builder.ToTable("VariacoesProduto");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Sku)
            .HasConversion(sku => sku.Codigo, valor => new Sku(valor))
            .HasColumnName("Sku")
            .HasMaxLength(50)
            .IsRequired();

        // Unicidade global de SKU — a checagem local em Produto.AdicionarVariacao (Etapa 3)
        // só garante que não haja duplicidade dentro do mesmo produto; este índice é o
        // backstop real contra duplicidade em todo o catálogo, à prova de condição de corrida.
        builder.HasIndex(v => v.Sku).IsUnique();

        builder.Property(v => v.CodigoBarras).HasMaxLength(20);
        builder.Property(v => v.Tamanho).HasMaxLength(20).IsRequired();
        builder.Property(v => v.Cor).HasMaxLength(40).IsRequired();

        builder.Property(v => v.PrecoAdicional)
            .HasConversion(
                dinheiro => dinheiro == null ? (decimal?)null : dinheiro.Valor,
                valor => valor == null ? null : new Dinheiro(valor.Value))
            .HasColumnName("PrecoAdicional")
            .HasColumnType("decimal(18,2)");

        builder.Property(v => v.Ativa).IsRequired();
    }
}
