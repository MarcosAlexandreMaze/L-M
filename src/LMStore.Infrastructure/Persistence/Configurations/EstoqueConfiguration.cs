using LMStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMStore.Infrastructure.Persistence.Configurations;

public class EstoqueConfiguration : IEntityTypeConfiguration<Estoque>
{
    public void Configure(EntityTypeBuilder<Estoque> builder)
    {
        builder.ToTable("Estoques");
        builder.HasKey(e => e.Id);

        // 1:1 com VariacaoProduto, mas só pelo Id — Estoque não tem navegação de volta
        // (e VariacaoProduto não sabe que Estoque existe). São agregados deliberadamente
        // separados (ver Etapa 4) para isolar a concorrência de escrita da compra.
        builder.HasIndex(e => e.VariacaoProdutoId).IsUnique();
        builder.HasOne<VariacaoProduto>().WithOne().HasForeignKey<Estoque>(e => e.VariacaoProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.QuantidadeDisponivel).IsRequired();
        builder.Property(e => e.QuantidadeReservada).IsRequired();

        // RowVersion não existe na entidade de domínio — é uma "shadow property": o EF
        // sabe dela, o banco tem a coluna, mas a classe Estoque nunca precisa saber que
        // controle de concorrência otimista existe. Serve de guarda secundária para
        // AjustarPara/Repor; a reserva no checkout (Etapa 7) usa UPDATE condicional atômico,
        // que dispensa esse token.
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasMany(e => e.Movimentos)
            .WithOne()
            .HasForeignKey(m => m.EstoqueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(e => e.Movimentos).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
