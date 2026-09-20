namespace LMStore.Application.DTOs.Auth;

// O que de fato sai no corpo JSON da resposta — sem refresh token aqui (vai num cookie
// httpOnly, nunca acessível via JavaScript no navegador — ver AuthController).
public record LoginResponse(string AccessToken, DateTime AccessTokenExpiraEm);
