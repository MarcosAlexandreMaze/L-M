using System.Text;
using System.Threading.RateLimiting;
using LMStore.Api.Filters;
using LMStore.Api.Middlewares;
using LMStore.Application;
using LMStore.Application.Common;
using LMStore.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options => options.Filters.Add<ValidacaoAutomaticaFilter>());
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SecaoConfiguracao).Get<JwtOptions>()
    ?? throw new InvalidOperationException($"Seção de configuração '{JwtOptions.SecaoConfiguracao}' não encontrada.");

if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
    throw new InvalidOperationException(
        "Jwt:SigningKey não configurada. Em desenvolvimento, defina via " +
        "'dotnet user-secrets set \"Jwt:SigningKey\" \"<valor>\"' — nunca em appsettings.json.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Linha de base para toda a API, particionada por IP.
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(contexto =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            factory: _ => new FixedWindowRateLimiterOptions { PermitLimit = 100, Window = TimeSpan.FromMinutes(1) }));

    // Limite bem mais rígido só para /api/auth — alvo típico de força bruta de
    // credenciais e credential stuffing.
    options.AddPolicy("auth", contexto => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
        factory: _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
});

var app = builder.Build();

// O middleware de exceção fica o mais externo possível — precisa envolver tudo que
// vem depois dele no pipeline para conseguir capturar qualquer coisa que escape.
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSecurityHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    // HSTS instrui o navegador a nunca mais tentar HTTP puro neste domínio — só faz
    // sentido fora do ambiente de desenvolvimento (com certificado dev local, HSTS
    // atrapalha mais do que ajuda ao alternar entre http/https durante o dev).
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
