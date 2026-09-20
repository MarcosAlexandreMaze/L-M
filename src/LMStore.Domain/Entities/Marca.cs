using LMStore.Domain.Common;
using LMStore.Domain.Exceptions;

namespace LMStore.Domain.Entities;

public class Marca : Entity
{
    public string Nome { get; private set; }
    public bool Ativa { get; private set; }

    public Marca(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome da marca é obrigatório.");

        Nome = nome.Trim();
        Ativa = true;
    }

    public void Renomear(string novoNome)
    {
        if (string.IsNullOrWhiteSpace(novoNome))
            throw new DomainException("O nome da marca é obrigatório.");

        Nome = novoNome.Trim();
    }

    public void Ativar()
    {
        if (Ativa)
            throw new DomainException("Esta marca já está ativa.");

        Ativa = true;
    }

    public void Desativar()
    {
        if (!Ativa)
            throw new DomainException("Esta marca já está inativa.");

        Ativa = false;
    }
}
