namespace EstateIQ.Exceptions;

/// <summary>
/// Represents an error caused by invalid input data.
/// </summary>
public class ValidationException : Exception
{
    public ValidationException(Dictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public Dictionary<string, string[]> Errors { get; }
}
