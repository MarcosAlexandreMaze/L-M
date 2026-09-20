using LMStore.Application.Common;
using LMStore.Application.Interfaces;
using LMStore.Domain.Interfaces;
using LMStore.Infrastructure.Persistence;
using LMStore.Infrastructure.Persistence.Repositories;
using LMStore.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LMStore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada.");

        services.AddDbContext<LMStoreDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IAdministradorRepository, AdministradorRepository>();
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<IMarcaRepository, MarcaRepository>();
        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<IEstoqueRepository, EstoqueRepository>();
        services.AddScoped<ICarrinhoRepository, CarrinhoRepository>();
        services.AddScoped<ICupomRepository, CupomRepository>();
        services.AddScoped<IPedidoRepository, PedidoRepository>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SecaoConfiguracao));
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
