namespace LMStore.Api.Middlewares;

public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;

            // Impede o navegador de "adivinhar" o Content-Type de uma resposta (evita
            // que um upload malicioso disfarçado seja executado como script).
            headers.Append("X-Content-Type-Options", "nosniff");

            // Impede que a API seja embutida num <iframe> de outro site (clickjacking).
            headers.Append("X-Frame-Options", "DENY");

            // Não vaza a URL completa de origem em requisições cross-site.
            headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

            // Desliga por padrão APIs de navegador sensíveis que esta API nunca usa.
            headers.Append("Permissions-Policy", "geolocation=(), camera=(), microphone=()");

            await next();
        });
    }
}
