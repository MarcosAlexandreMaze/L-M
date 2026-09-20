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
        services.AddValidatorsFromAssemblyContaining(typeof(DependencyInjection));

        return services;
    }
}
