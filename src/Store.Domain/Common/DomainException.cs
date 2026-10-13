namespace Store.Domain.Common;

/// <summary>
/// Raised when an operation would break a business rule.
/// The API maps it to a 4xx response instead of a server error.
/// </summary>
public class DomainException(string message) : Exception(message);
