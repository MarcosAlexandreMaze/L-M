namespace LMStore.Domain.Exceptions;

public abstract class LMStoreException : Exception
{
    protected LMStoreException(string message) : base(message)
    {
    }
}
