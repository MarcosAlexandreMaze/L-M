using LMStore.Domain.Common;
using LMStore.Domain.Exceptions;

namespace LMStore.Domain.Entities;

public class RefreshToken : Entity
{
    public Guid UsuarioId { get; }
    public string TokenHash { get; }
    public DateTime ExpiraEm { get; }
    public bool Revogado { get; private set; }
    public DateTime? RevogadoEm { get; private set; }

    internal RefreshToken(Guid usuarioId, string tokenHash, DateTime expiraEm)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new DomainException("O hash do refresh token é obrigatório.");

        UsuarioId = usuarioId;
        TokenHash = tokenHash;
        ExpiraEm = expiraEm;
        Revogado = false;
    }

    public bool EstaValido() => !Revogado && DateTime.UtcNow < ExpiraEm;

    internal void Revogar()
    {
        if (Revogado)
            throw new DomainException("Este refresh token já foi revogado.");

        Revogado = true;
        RevogadoEm = DateTime.UtcNow;
    }
}
