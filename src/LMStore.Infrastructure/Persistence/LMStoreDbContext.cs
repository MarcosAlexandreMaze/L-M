using System.Reflection;
using LMStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMStore.Infrastructure.Persistence;

public class LMStoreDbContext(DbContextOptions<LMStoreDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Administrador> Administradores => Set<Administrador>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Marca> Marcas => Set<Marca>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Roupa> Roupas => Set<Roupa>();
    public DbSet<Tenis> Tenis => Set<Tenis>();
    public DbSet<Estoque> Estoques => Set<Estoque>();
    public DbSet<Carrinho> Carrinhos => Set<Carrinho>();
    public DbSet<Cupom> Cupons => Set<Cupom>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
