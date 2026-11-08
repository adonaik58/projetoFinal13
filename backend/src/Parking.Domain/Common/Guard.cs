namespace Parking.Domain.Common;

internal static class Guard
{
    public static string Text(string? value, int minLength, int maxLength, string code, string message)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length < minLength || trimmed.Length > maxLength)
        {
            throw new DomainException(code, message);
        }

        return trimmed;
    }
}
