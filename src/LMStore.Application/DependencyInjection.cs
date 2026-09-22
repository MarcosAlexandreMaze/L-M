using FluentValidation;
using LMStore.Application.Interfaces;
using LMStore.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LMStore.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICatalogoService, CatalogoService>();
        services.AddScoped<ICarrinhoService, CarrinhoService>();
        services.AddScoped<ICupomService, CupomService>();
        services.AddScoped<IPedidoService, PedidoService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IAdminProdutoService, AdminProdutoService>();
        services.AddScoped<IAdminCategoriaService, AdminCategoriaService>();
        services.AddScoped<IAdminMarcaService, AdminMarcaService>();
        services.AddScoped<IAdminEstoqueService, AdminEstoqueService>();
        services.AddScoped<IAdminCupomService, AdminCupomService>();
        services.AddScoped<IAdminPedidoService, AdminPedidoService>();
        services.AddScoped<IAdminClienteService, AdminClienteService>();
        services.AddScoped<IAdministradorService, AdministradorService>();
        services.AddValidatorsFromAssemblyContaining(typeof(DependencyInjection));

        return services;
    }
}
