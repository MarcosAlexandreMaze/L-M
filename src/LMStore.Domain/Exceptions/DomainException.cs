namespace LMStore.Domain.Exceptions;

public class DomainException : LMStoreException
{
    public DomainException(string message) : base(message)
    {
    }
}
