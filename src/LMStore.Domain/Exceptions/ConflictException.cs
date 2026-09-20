namespace LMStore.Domain.Exceptions;

public class ConflictException : LMStoreException
{
    public ConflictException(string message) : base(message)
    {
    }
}
