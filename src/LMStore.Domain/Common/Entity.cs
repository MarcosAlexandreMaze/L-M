namespace LMStore.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected init; }
    public DateTime CriadoEm { get; protected init; }

    protected Entity()
    {
        Id = Guid.CreateVersion7();
        CriadoEm = DateTime.UtcNow;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity outra || GetType() != outra.GetType())
            return false;

        return ReferenceEquals(this, outra) || Id == outra.Id;
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity? esquerda, Entity? direita)
    {
        if (esquerda is null && direita is null)
            return true;

        if (esquerda is null || direita is null)
            return false;

        return esquerda.Equals(direita);
    }

    public static bool operator !=(Entity? esquerda, Entity? direita) => !(esquerda == direita);
}
