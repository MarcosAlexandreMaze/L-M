using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using LMStore.Api.Filters;
using LMStore.Api.Middlewares;
using LMStore.Api.Services;
using LMStore.Application;
using LMStore.Application.Common;
using LMStore.Application.Interfaces;
using LMStore.Infrastructure;
using LMStore.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// JsonStringEnumConverter: sem isso, todo enum (FormaPagamento, GeneroProduto...) num
// corpo de requisição precisaria ser enviado como número (0, 1, 2...) — nada óbvio pra
// quem consome a API. Com o converter, tanto entrada quanto saída aceitam/devolvem o
// nome do enum como texto ("Pix", "Unissex"...).
builder.Services.AddControllers(options => options.Filters.Add<ValidacaoAutomaticaFilter>())
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "L&M Store API",
        Version = "v1",
        Description = "API de e-commerce para roupas fitness e tênis (camisetas, shorts, tênis...). " +
            "Registro público só cria Cliente — use as credenciais do administrador semeado em " +
            "desenvolvimento para os endpoints /api/admin/*."
    });

    // Botão "Authorize" no topo da tela: cole só o token (sem "Bearer "), e o Swagger
    // passa a mandar o header Authorization automaticamente em todo "Try it out".
    var esquemaJwt = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Cole aqui só o token retornado por /api/auth/login ou /api/auth/registrar " +
            "(sem o prefixo 'Bearer ' — o Swagger adiciona sozinho)."
    };

    options.AddSecurityDefinition("Bearer", esquemaJwt);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

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
        // Sem isso, o handler remapeia silenciosamente claims curtas ("sub", "email")
        // para URIs antigas do WS-Federation (ClaimTypes.NameIdentifier etc.) ao montar
        // o ClaimsPrincipal — então procurar por JwtRegisteredClaimNames.Sub depois de
        // autenticado não encontra nada. Mantém os nomes de claim exatamente como foram
        // emitidos em JwtTokenGenerator.
        options.MapInboundClaims = false;

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

const string PoliticaCors = "LMStoreCors";

builder.Services.AddCors(options =>
{
    options.AddPolicy(PoliticaCors, policy =>
    {
        // Sem frontend ainda — a lista de origens vem da configuração (Cors:AllowedOrigins
        // em appsettings/User Secrets), vazia por padrão. AllowCredentials() é obrigatório
        // porque o refresh token viaja em cookie (Etapa 8), e a spec de CORS proíbe
        // combinar AllowAnyOrigin() com AllowCredentials() — por isso NÃO existe um
        // fallback permissivo "libera tudo" aqui, nem em desenvolvimento: até a origem do
        // futuro frontend ser configurada, chamadas de API vindas de outro domínio/porta
        // simplesmente não vão funcionar (Swagger UI e o próprio navegador direto no Kestrel
        // continuam funcionando normalmente, porque são same-origin e não passam por CORS).
        var origensPermitidas = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        if (origensPermitidas.Length > 0)
            policy.WithOrigins(origensPermitidas).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

var app = builder.Build();

// O middleware de exceção fica o mais externo possível — precisa envolver tudo que
// vem depois dele no pipeline para conseguir capturar qualquer coisa que escape.
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSecurityHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "L&M Store API v1");
        options.DocumentTitle = "L&M Store API";
    });

    // Só em desenvolvimento: aplica migrations pendentes e garante os dados iniciais
    // (marca L&M + os dois produtos, e o administrador padrão) sem precisar rodar
    // `dotnet ef` ou criar o primeiro admin na mão toda vez.
    using var escopoInicializacao = app.Services.CreateScope();
    var dbContext = escopoInicializacao.ServiceProvider.GetRequiredService<LMStoreDbContext>();
    var passwordHasher = escopoInicializacao.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await dbContext.Database.MigrateAsync();
    await DbSeeder.SeedAsync(dbContext);
    await DbSeeder.SeedAdminAsync(dbContext, passwordHasher);
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
app.UseCors(PoliticaCors);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
