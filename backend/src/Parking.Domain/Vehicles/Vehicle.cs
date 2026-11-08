using System.Text.RegularExpressions;
using Parking.Domain.Common;

namespace Parking.Domain.Vehicles;

public sealed partial class Vehicle : AuditableEntity<int>
{
    public const int PlateMaxLength = 20;
    public const int ColorLength = 7;

    private Vehicle()
    {
    }

    public string Plate { get; private set; } = null!;

    public int BrandId { get; private set; }

    public VehicleBrand Brand { get; private set; } = null!;

    public int? ModelId { get; private set; }

    public VehicleModel? Model { get; private set; }

    /// <summary>Hex colour <c>#RRGGBB</c>.</summary>
    public string Color { get; private set; } = null!;

    public static Vehicle Create(string plate, VehicleBrand brand, VehicleModel? model, string color)
    {
        var normalizedPlate = NormalizePlate(plate);
        if (!IsValidPlate(normalizedPlate))
        {
            throw new DomainException("vehicle.invalid_plate", "Matrícula inválida.");
        }

        var vehicle = new Vehicle { Plate = normalizedPlate };
        vehicle.Update(brand, model, color);
        return vehicle;
    }

    public static string NormalizePlate(string plate) =>
        WhitespacePattern().Replace(plate.Trim().ToUpperInvariant(), " ");

    public static bool IsValidPlate(string? plate) => plate is not null && PlatePattern().IsMatch(plate);

    public static bool IsValidColor(string? color) => color is not null && ColorPattern().IsMatch(color);

    public void Update(VehicleBrand brand, VehicleModel? model, string color)
    {
        if (model is not null && model.BrandId != brand.Id)
        {
            throw new DomainException("vehicle.model_brand_mismatch", "O modelo não pertence à marca indicada.");
        }

        if (!IsValidColor(color))
        {
            throw new DomainException("vehicle.invalid_color", "Cor inválida (formato #RRGGBB).");
        }

        Brand = brand;
        BrandId = brand.Id;
        Model = model;
        ModelId = model?.Id;
        Color = color.ToUpperInvariant();
    }

    [GeneratedRegex("^[A-Z0-9][A-Z0-9 -]{0,18}[A-Z0-9]$", RegexOptions.CultureInvariant)]
    private static partial Regex PlatePattern();

    [GeneratedRegex("^#[0-9A-Fa-f]{6}$", RegexOptions.CultureInvariant)]
    private static partial Regex ColorPattern();

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex WhitespacePattern();
}
