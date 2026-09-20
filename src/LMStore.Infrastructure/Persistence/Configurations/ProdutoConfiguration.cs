using System.Text.Json;
using LMStore.Domain.Entities;
using LMStore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMStore.Infrastructure.Persistence.Configurations;

public class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("Produtos");
        builder.HasKey(p => p.Id);

        // Table-Per-Hierarchy: Produto, Roupa e Tenis vivem na mesma tabela. A coluna
        // "TipoProduto" diz qual subtipo cada linha representa — sem ela, o EF criaria
        // uma coluna "Discriminator" com o nome completo da classe .NET, bem menos legível.
        builder.HasDiscriminator<string>("TipoProduto")
            .HasValue<Roupa>("Roupa")
            .HasValue<Tenis>("Tenis");

        builder.Property(p => p.Nome).HasMaxLength(150).IsRequired();
        builder.Property(p => p.Descricao).HasMaxLength(2000).IsRequired();

        builder.Property(p => p.Preco)
            .HasConversion(dinheiro => dinheiro.Valor, valor => new Dinheiro(valor))
            .HasColumnName("Preco")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.PrecoPromocional)
            .HasConversion(
                dinheiro => dinheiro == null ? (decimal?)null : dinheiro.Valor,
                valor => valor == null ? null : new Dinheiro(valor.Value))
            .HasColumnName("PrecoPromocional")
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Genero).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(p => p.Ativo).IsRequired();

        builder.HasIndex(p => p.MarcaId);
        builder.HasOne<Marca>().WithMany().HasForeignKey(p => p.MarcaId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.CategoriaId);
        builder.HasOne<Categoria>().WithMany().HasForeignKey(p => p.CategoriaId).OnDelete(DeleteBehavior.Restrict);

        // Imagens é List<string> sem contrapartida natural em coluna relacional simples;
        // serializamos como JSON num único nvarchar(max) em vez de criar uma tabela à parte
        // só para URLs — não há necessidade de consultar por imagem individualmente.
        // Convertida via HasConversion, Imagens é uma propriedade escalar (a coleção
        // inteira vira uma string JSON) — não uma navegação. UsePropertyAccessMode entra
        // direto na própria PropertyBuilder, não via builder.Navigation(...).
        var imagensProperty = builder.Property(p => p.Imagens)
            .HasConversion(
                imagens => JsonSerializer.Serialize(imagens, (JsonSerializerOptions?)null),
                json => JsonSerializer.Deserialize<List<string>>(json, (JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnName("Imagens")
            .HasColumnType("nvarchar(max)")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Sem um ValueComparer explícito, o EF não sabe comparar o "antes" e "depois" de
        // uma List<string> convertida (compararia por referência, não por conteúdo) —
        // mudanças feitas via AdicionarImagem/RemoverImagem não seriam detectadas no
        // change tracking. Este comparer diz como checar igualdade, gerar hash e tirar
        // um "retrato" (snapshot) da lista para essa comparação.
        imagensProperty.Metadata.SetValueComparer(new ValueComparer<IReadOnlyList<string>>(
            (listaA, listaB) => (listaA ?? new List<string>()).SequenceEqual(listaB ?? new List<string>()),
            lista => lista.Aggregate(0, (hash, url) => HashCode.Combine(hash, url.GetHashCode())),
            lista => lista.ToList()));

        builder.HasMany(p => p.Variacoes)
            .WithOne()
            .HasForeignKey(v => v.ProdutoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Variacoes).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
