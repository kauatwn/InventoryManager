namespace InventoryManager.Domain.Exceptions;

public sealed class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors) : base("Validation failed")
    {
        Errors = errors;
    }

    public ValidationException(string field, string message) : base("Validation failed")
    {
        Errors = new Dictionary<string, string[]>
        {
            { field, [message] }
        };
    }
}