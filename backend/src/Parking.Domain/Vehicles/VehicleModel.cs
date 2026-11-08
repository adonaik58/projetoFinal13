using Parking.Domain.Common;

namespace Parking.Domain.Vehicles;

public sealed class VehicleModel : Entity<int>
{
    public const int NameMaxLength = 80;

    private VehicleModel()
    {
    }

    public int BrandId { get; private set; }

    public string Name { get; private set; } = null!;

    public static VehicleModel Create(int brandId, string name) =>
        new()
        {
            BrandId = brandId,
            Name = Guard.Text(name, 1, NameMaxLength, "catalog.invalid_name", "Nome de modelo inválido."),
        };
}
