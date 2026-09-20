namespace LMStore.Application.Common;

public class JwtOptions
{
    public const string SecaoConfiguracao = "Jwt";

    // SigningKey nunca fica em appsettings.json — só em User Secrets (dev) ou
    // variável de ambiente/cofre de segredos (produção). Ver Etapa 8 no histórico
    // da conversa para o comando exato usado em desenvolvimento.
    public string SigningKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenMinutos { get; set; } = 15;
    public int RefreshTokenDias { get; set; } = 7;
}
