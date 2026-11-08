namespace Parking.Application.Common.Exceptions;

/// <summary>Base type for expected failures that map to a specific HTTP status.</summary>
public abstract class AppException : Exception
{
    protected AppException(string code, string message)
        : base(message) => Code = code;

    public string Code { get; }
}

public sealed class NotFoundException(string code, string message) : AppException(code, message)
{
    public static NotFoundException For(string entity, object id) =>
        new($"{entity.ToLowerInvariant()}.not_found", $"{entity} '{id}' não encontrado.");
}

public sealed class ConflictException(string code, string message) : AppException(code, message);

public sealed class ForbiddenException(string code, string message) : AppException(code, message);

public sealed class UnauthorizedException(string code, string message) : AppException(code, message);

public sealed class TooManyRequestsException(string code, string message, TimeSpan retryAfter) : AppException(code, message)
{
    public TimeSpan RetryAfter { get; } = retryAfter;
}
