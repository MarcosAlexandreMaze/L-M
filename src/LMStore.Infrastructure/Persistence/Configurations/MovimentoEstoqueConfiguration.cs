using LMStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMStore.Infrastructure.Persistence.Configurations;

public class MovimentoEstoqueConfiguration : IEntityTypeConfiguration<MovimentoEstoque>
{
    public void Configure(EntityTypeBuilder<MovimentoEstoque> builder)
    {
        builder.ToTable("MovimentosEstoque");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Tipo).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(m => m.Quantidade).IsRequired();
        builder.Property(m => m.Motivo).HasMaxLength(300).IsRequired();

        // PedidoId é só informativo (auditoria) — não vira FK para Pedidos, pelo mesmo
        // motivo do ItemPedido: um movimento de estoque não pode nunca ser bloqueado ou
        // afetado por mudanças no histórico de pedidos.
        builder.Property(m => m.PedidoId);
    }
}
