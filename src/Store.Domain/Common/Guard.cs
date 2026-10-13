namespace Store.Domain.Common;

/// <summary>
/// Small helpers for validating entity input. They throw <see cref="DomainException"/>
/// so invalid data never reaches a valid entity state.
/// </summary>
internal static class Guard
{
    public static string NotEmpty(string? value, string field, int maxLength)
    {
        var trimmed = value?.Trim() ?? string.Empty;

        if (trimmed.Length == 0)
        {
            throw new DomainException($"{field} is required.");
        }

        if (trimmed.Length > maxLength)
        {
            throw new DomainException($"{field} must be at most {maxLength} characters long.");
        }

        return trimmed;
    }
}
