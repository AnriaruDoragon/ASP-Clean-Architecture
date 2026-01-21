namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when a requested entity is not found.
/// </summary>
public class NotFoundException : DomainException
{
    public NotFoundException()
        : base("The requested resource was not found.")
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.")
    {
    }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
