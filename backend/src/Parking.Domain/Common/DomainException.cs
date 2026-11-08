namespace Parking.Domain.Common;

/// <summary>Raised when an operation would violate a business invariant.</summary>
public sealed class DomainException : Exception
{
    public DomainException(string code, string message)
        : base(message) => Code = code;

    public DomainException()
        : this("domain.error", "Operação inválida.")
    {
    }

    public DomainException(string message)
        : this("domain.error", message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException) => Code = "domain.error";

    public string Code { get; }
}
