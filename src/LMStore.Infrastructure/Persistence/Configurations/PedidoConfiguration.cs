using LMStore.Domain.Entities;
using LMStore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMStore.Infrastructure.Persistence.Configurations;

public class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("Pedidos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Numero).HasMaxLength(30).IsRequired();
        builder.HasIndex(p => p.Numero).IsUnique();

        // Pedido não referencia Cliente como FK (histórico não pode ser afetado por
        // exclusão de cliente) — só um índice para consultas ("meus pedidos").
        builder.HasIndex(p => p.ClienteId);

        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(p => p.FormaPagamento).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(p => p.MotivoCancelamento).HasMaxLength(500);
        builder.Property(p => p.CupomAplicadoId);

        foreach (var propriedade in new[] { nameof(Pedido.Subtotal), nameof(Pedido.DescontoCupom), nameof(Pedido.ValorFrete), nameof(Pedido.Total) })
        {
            builder.Property<Dinheiro>(propriedade)
                .HasConversion(dinheiro => dinheiro.Valor, valor => new Dinheiro(valor))
                .HasColumnType("decimal(18,2)")
                .IsRequired();
        }

        // EnderecoEntrega é um Value Object com vários campos (não um único valor como
        // Cpf/Email) — em vez de HasConversion (que só mapeia para UMA coluna), usamos
        // OwnsOne: os campos do endereço viram colunas na própria tabela Pedidos, sem
        // tabela nem FK separada. Cep, por sua vez, ainda precisa da própria conversão
        // dentro do tipo possuído.
        builder.OwnsOne(p => p.EnderecoEntrega, endereco =>
        {
            endereco.Property(e => e.Logradouro).HasColumnName("Entrega_Logradouro").HasMaxLength(200).IsRequired();
            endereco.Property(e => e.Numero).HasColumnName("Entrega_Numero").HasMaxLength(20).IsRequired();
            endereco.Property(e => e.Complemento).HasColumnName("Entrega_Complemento").HasMaxLength(200);
            endereco.Property(e => e.Bairro).HasColumnName("Entrega_Bairro").HasMaxLength(100).IsRequired();
            endereco.Property(e => e.Cidade).HasColumnName("Entrega_Cidade").HasMaxLength(100).IsRequired();
            endereco.Property(e => e.Estado).HasColumnName("Entrega_Estado").HasMaxLength(2).IsRequired();
            endereco.Property(e => e.Pais).HasColumnName("Entrega_Pais").HasMaxLength(60).IsRequired();

            endereco.Property(e => e.Cep)
                .HasConversion(cep => cep.Numero, valor => new Cep(valor))
                .HasColumnName("Entrega_Cep")
                .HasMaxLength(8)
                .IsRequired();
        });

        builder.HasMany(p => p.Itens)
            .WithOne()
            .HasForeignKey(i => i.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Itens).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(p => p.Pagamentos)
            .WithOne()
            .HasForeignKey(pg => pg.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Pagamentos).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Propriedade computada (_pagamentos.LastOrDefault()) — sem Ignore(), o EF a
        // interpretaria como mais uma navegação para Pagamento e entraria em conflito
        // com a coleção Pagamentos já mapeada acima.
        builder.Ignore(p => p.PagamentoAtual);
    }
}
