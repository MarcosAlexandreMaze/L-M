using LMStore.Domain.Entities;
using LMStore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMStore.Infrastructure.Persistence.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");
        builder.HasKey(c => c.Id);

        // Cliente não tem navegação para Usuario no domínio (são agregados independentes,
        // ligados só pelo Id — ver explicação da Etapa 2). O relacionamento ainda existe
        // no banco: configuramos a FK sem nenhuma propriedade de navegação nos dois lados.
        builder.HasIndex(c => c.UsuarioId).IsUnique();
        builder.HasOne<Usuario>().WithOne().HasForeignKey<Cliente>(c => c.UsuarioId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(c => c.Nome).HasMaxLength(150).IsRequired();

        builder.Property(c => c.Cpf)
            .HasConversion(cpf => cpf.Numero, valor => new Cpf(valor))
            .HasColumnName("Cpf")
            .HasMaxLength(11)
            .IsRequired();

        builder.HasIndex(c => c.Cpf).IsUnique();

        builder.Property(c => c.Telefone).HasMaxLength(20).IsRequired();
        builder.Property(c => c.DataNascimento).IsRequired();

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Enderecos é exposta como IReadOnlyList<Endereco> (sem setter) sobre o campo
        // privado _enderecos — o EF precisa usar o campo diretamente para materializar
        // e rastrear a coleção, já que não existe um setter público para chamar.
        builder.HasMany(c => c.Enderecos)
            .WithOne()
            .HasForeignKey(e => e.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.Enderecos).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
