namespace LMStore.Domain.Exceptions;

public class NotFoundException : LMStoreException
{
    public NotFoundException(string message) : base(message)
    {
    }
}
