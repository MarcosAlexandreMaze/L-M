namespace LMStore.Application.DTOs.Auth;

// Resultado interno da Application — carrega o refresh token em texto puro porque
// quem chama (o Controller) precisa dele para colocar num cookie httpOnly. Ele NUNCA
// deve ser serializado direto numa resposta JSON — ver AuthController.
public record AuthResultado(string AccessToken, DateTime AccessTokenExpiraEm, string RefreshToken, DateTime RefreshTokenExpiraEm);
