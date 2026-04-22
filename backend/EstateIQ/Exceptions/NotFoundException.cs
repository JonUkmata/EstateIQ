namespace EstateIQ.Exceptions;

/// <summary>
/// Represents an error when an expected entity cannot be found.
/// </summary>
public class NotFoundException(string message) : Exception(message)
{
}
