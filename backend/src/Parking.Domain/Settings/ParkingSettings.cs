using Parking.Domain.Common;

namespace Parking.Domain.Settings;

/// <summary>Singleton row holding the tariff and the parking capacity.</summary>
public sealed class ParkingSettings : Entity<int>
{
    public const int SingletonId = 1;
    public const decimal MaxRatePerMinute = 1_000_000m;
    public const int MaxFreeHours = 720;
    public const int MaxCapacity = 100_000;

    private ParkingSettings()
    {
    }

    public decimal RatePerMinute { get; private set; }

    public int FreeHours { get; private set; }

    public int MaxSpaces { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public int? UpdatedByUserId { get; private set; }

    public int FreeMinutes => FreeHours * 60;

    public static ParkingSettings Create(decimal ratePerMinute, int freeHours, int maxSpaces, DateTime now)
    {
        var settings = new ParkingSettings { Id = SingletonId };
        settings.Apply(ratePerMinute, freeHours, maxSpaces);
        settings.UpdatedAt = now;
        return settings;
    }

    public void Update(decimal ratePerMinute, int freeHours, int maxSpaces, int currentSpaceCount, int updatedByUserId, DateTime now)
    {
        if (maxSpaces < currentSpaceCount)
        {
            throw new DomainException(
                "settings.capacity_below_existing",
                $"A capacidade não pode ser inferior ao número de espaços existentes ({currentSpaceCount}).");
        }

        Apply(ratePerMinute, freeHours, maxSpaces);
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = now;
    }

    private void Apply(decimal ratePerMinute, int freeHours, int maxSpaces)
    {
        if (ratePerMinute <= 0 || ratePerMinute > MaxRatePerMinute || decimal.Round(ratePerMinute, 2) != ratePerMinute)
        {
            throw new DomainException("settings.invalid_rate", "Tarifa por minuto inválida.");
        }

        if (freeHours is < 0 or > MaxFreeHours)
        {
            throw new DomainException("settings.invalid_free_hours", "Número de horas grátis inválido.");
        }

        if (maxSpaces is < 1 or > MaxCapacity)
        {
            throw new DomainException("settings.invalid_capacity", "Capacidade máxima inválida.");
        }

        RatePerMinute = ratePerMinute;
        FreeHours = freeHours;
        MaxSpaces = maxSpaces;
    }
}
