using System.Text.RegularExpressions;
using Parking.Domain.Common;

namespace Parking.Domain.Customers;

public sealed partial class Customer : AuditableEntity<int>
{
    public const int FullNameMaxLength = 150;
    public const int NationalIdLength = 14;

    private Customer()
    {
    }

    public string FullName { get; private set; } = null!;

    /// <summary>Angolan identity card number (Bilhete de Identidade), e.g. <c>123456789LA123</c>.</summary>
    public string NationalId { get; private set; } = null!;

    public DateOnly BirthDate { get; private set; }

    public static Customer Create(string fullName, string nationalId, DateOnly birthDate, DateOnly today)
    {
        var normalizedId = NormalizeNationalId(nationalId);
        if (!IsValidNationalId(normalizedId))
        {
            throw new DomainException("customer.invalid_national_id", "Bilhete de Identidade inválido.");
        }

        var customer = new Customer { NationalId = normalizedId };
        customer.Update(fullName, birthDate, today);
        return customer;
    }

    public static string NormalizeNationalId(string nationalId) => nationalId.Trim().ToUpperInvariant();

    public static bool IsValidNationalId(string? nationalId) =>
        nationalId is not null && NationalIdPattern().IsMatch(nationalId);

    public void Update(string fullName, DateOnly birthDate, DateOnly today)
    {
        FullName = Guard.Text(fullName, 2, FullNameMaxLength, "customer.invalid_name", "Nome do cliente inválido.");
        if (birthDate >= today || birthDate < today.AddYears(-120))
        {
            throw new DomainException("customer.invalid_birth_date", "Data de nascimento inválida.");
        }

        BirthDate = birthDate;
    }

    public static int AgeOn(DateOnly birthDate, DateOnly today)
    {
        var age = today.Year - birthDate.Year;
        return birthDate > today.AddYears(-age) ? age - 1 : age;
    }

    public int AgeAt(DateOnly today) => AgeOn(BirthDate, today);

    [GeneratedRegex("^[0-9]{9}[A-Z]{2}[0-9]{3}$", RegexOptions.CultureInvariant)]
    private static partial Regex NationalIdPattern();
}
