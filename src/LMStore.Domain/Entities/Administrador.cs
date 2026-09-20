using LMStore.Domain.Common;
using LMStore.Domain.Exceptions;

namespace LMStore.Domain.Entities;

public class Administrador : Entity
{
    public Guid UsuarioId { get; }
    public string Nome { get; private set; }
    public string? Cargo { get; private set; }

    public Administrador(Guid usuarioId, string nome, string? cargo = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do administrador é obrigatório.");

        UsuarioId = usuarioId;
        Nome = nome.Trim();
        Cargo = string.IsNullOrWhiteSpace(cargo) ? null : cargo.Trim();
    }

    public void AtualizarDados(string nome, string? cargo)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do administrador é obrigatório.");

        Nome = nome.Trim();
        Cargo = string.IsNullOrWhiteSpace(cargo) ? null : cargo.Trim();
    }
}
