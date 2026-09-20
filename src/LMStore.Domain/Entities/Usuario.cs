using LMStore.Domain.Common;
using LMStore.Domain.Enums;
using LMStore.Domain.Exceptions;
using LMStore.Domain.ValueObjects;

namespace LMStore.Domain.Entities;

public class Usuario : Entity
{
    private readonly List<RefreshToken> _refreshTokens = [];

    public Email Email { get; private set; }
    public string SenhaHash { get; private set; }
    public PerfilUsuario Perfil { get; }
    public bool Ativo { get; private set; }
    public IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    public Usuario(Email email, string senhaHash, PerfilUsuario perfil)
    {
        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new DomainException("A senha não pode ser vazia.");

        Email = email;
        SenhaHash = senhaHash;
        Perfil = perfil;
        Ativo = true;
    }

    public void DefinirSenha(string novoHash)
    {
        if (string.IsNullOrWhiteSpace(novoHash))
            throw new DomainException("A senha não pode ser vazia.");

        SenhaHash = novoHash;
    }

    public void AlterarEmail(Email novoEmail)
    {
        Email = novoEmail;
    }

    public void Ativar()
    {
        if (Ativo)
            throw new DomainException("Este usuário já está ativo.");

        Ativo = true;
    }

    public void Desativar()
    {
        if (!Ativo)
            throw new DomainException("Este usuário já está inativo.");

        Ativo = false;
    }

    // O valor bruto do token nunca é guardado — só o hash (SHA-256 é suficiente aqui,
    // diferente da senha: um refresh token já nasce com alta entropia aleatória, não é
    // uma senha fraca escolhida por humano, então não precisa do custo computacional do
    // BCrypt). Quem chama já entra com o hash pronto.
    public RefreshToken EmitirRefreshToken(string tokenHash, TimeSpan duracao)
    {
        var refreshToken = new RefreshToken(Id, tokenHash, DateTime.UtcNow.Add(duracao));
        _refreshTokens.Add(refreshToken);
        return refreshToken;
    }

    // Rotação: todo uso de refresh token invalida o token antigo e emite um novo. Se um
    // token roubado for usado depois que o legítimo já rotacionou, ele vai falhar (já
    // revogado) — um sinal de possível comprometimento que o Application pode tratar
    // futuramente revogando todos os tokens do usuário.
    public RefreshToken RotacionarRefreshToken(string tokenHashAtual, string novoTokenHash, TimeSpan duracao)
    {
        var tokenAtual = _refreshTokens.FirstOrDefault(t => t.TokenHash == tokenHashAtual)
            ?? throw new NotFoundException("Refresh token não encontrado.");

        if (!tokenAtual.EstaValido())
            throw new DomainException("Este refresh token não é mais válido.");

        tokenAtual.Revogar();
        return EmitirRefreshToken(novoTokenHash, duracao);
    }
}
