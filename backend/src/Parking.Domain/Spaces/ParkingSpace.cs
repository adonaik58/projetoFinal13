using System.Globalization;
using System.Text.RegularExpressions;
using Parking.Domain.Common;
using Parking.Domain.Users;

namespace Parking.Domain.Spaces;

public sealed partial class ParkingSpace : AuditableEntity<int>
{
    public const int CodeMaxLength = 7;
    public const int SectionMaxLength = 3;
    public const int MaxNumber = 9999;

    private ParkingSpace()
    {
    }

    /// <summary>Human code, e.g. <c>A1</c> = section <c>A</c> + number <c>1</c>.</summary>
    public string Code { get; private set; } = null!;

    public string Section { get; private set; } = null!;

    public int Number { get; private set; }

    public SpaceStatus Status { get; private set; }

    /// <summary>Optimistic concurrency token, incremented on every state transition.</summary>
    public int Version { get; private set; }

    /// <summary>User who added the space; <c>null</c> for spaces imported or created before this was recorded.</summary>
    public int? CreatedByUserId { get; private set; }

    public User? CreatedBy { get; private set; }

    public static ParkingSpace Create(string code, int? createdByUserId = null)
    {
        var (section, number) = ParseCode(code);
        return Create(section, number, createdByUserId);
    }

    public static ParkingSpace Create(string section, int number, int? createdByUserId = null)
    {
        var normalizedSection = section.Trim().ToUpperInvariant();
        if (!SectionPattern().IsMatch(normalizedSection) || number is < 1 or > MaxNumber)
        {
            throw new DomainException("space.invalid_code", "Código de espaço inválido (ex.: A1).");
        }

        return new ParkingSpace
        {
            Section = normalizedSection,
            Number = number,
            Code = string.Create(CultureInfo.InvariantCulture, $"{normalizedSection}{number}"),
            Status = SpaceStatus.Free,
            CreatedByUserId = createdByUserId,
        };
    }

    public static bool IsValidCode(string? code) => code is not null && CodePattern().IsMatch(code.Trim().ToUpperInvariant());

    public static (string Section, int Number) ParseCode(string code)
    {
        var match = CodePattern().Match(code.Trim().ToUpperInvariant());
        if (!match.Success)
        {
            throw new DomainException("space.invalid_code", "Código de espaço inválido (ex.: A1).");
        }

        return (match.Groups[1].Value, int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture));
    }

    public void Occupy()
    {
        if (Status != SpaceStatus.Free)
        {
            throw new DomainException("ticket.space_not_free", $"O espaço {Code} não está livre.");
        }

        Transition(SpaceStatus.Occupied);
    }

    public void Release()
    {
        if (Status != SpaceStatus.Occupied)
        {
            throw new DomainException("space.not_occupied", $"O espaço {Code} não está ocupado.");
        }

        Transition(SpaceStatus.Free);
    }

    public void SetMaintenance()
    {
        EnsureNotOccupied();
        Transition(SpaceStatus.Maintenance);
    }

    public void SetAvailable()
    {
        EnsureNotOccupied();
        Transition(SpaceStatus.Free);
    }

    public void EnsureNotOccupied()
    {
        if (Status == SpaceStatus.Occupied)
        {
            throw new DomainException("space.occupied", $"O espaço {Code} está ocupado.");
        }
    }

    private void Transition(SpaceStatus status)
    {
        Status = status;
        Version++;
    }

    [GeneratedRegex("^[A-Z]{1,3}$", RegexOptions.CultureInvariant)]
    private static partial Regex SectionPattern();

    [GeneratedRegex("^([A-Z]{1,3})([1-9][0-9]{0,3})$", RegexOptions.CultureInvariant)]
    private static partial Regex CodePattern();
}
