using LMStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMStore.Infrastructure.Persistence.Configurations;

public class AdministradorConfiguration : IEntityTypeConfiguration<Administrador>
{
    public void Configure(EntityTypeBuilder<Administrador> builder)
    {
        builder.ToTable("Administradores");
        builder.HasKey(a => a.Id);

        builder.HasIndex(a => a.UsuarioId).IsUnique();
        builder.HasOne<Usuario>().WithOne().HasForeignKey<Administrador>(a => a.UsuarioId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(a => a.Nome).HasMaxLength(150).IsRequired();
        builder.Property(a => a.Cargo).HasMaxLength(100);
    }
}
