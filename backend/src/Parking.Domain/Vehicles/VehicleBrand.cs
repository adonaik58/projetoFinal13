using Parking.Domain.Common;

namespace Parking.Domain.Vehicles;

public sealed class VehicleBrand : Entity<int>
{
    public const int NameMaxLength = 80;

    private VehicleBrand()
    {
    }

    public string Name { get; private set; } = null!;

    public static VehicleBrand Create(string name) =>
        new() { Name = Guard.Text(name, 1, NameMaxLength, "catalog.invalid_name", "Nome de marca inválido.") };
}
