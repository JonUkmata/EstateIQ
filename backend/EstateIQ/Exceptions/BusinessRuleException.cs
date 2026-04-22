namespace EstateIQ.Exceptions;

/// <summary>
/// Represents an error caused by business rule violations.
/// </summary>
public class BusinessRuleException(string message) : Exception(message)
{
}
