using System.Reflection;
using Parking.Domain.Customers;
using Parking.Domain.Spaces;
using Parking.Domain.Vehicles;

namespace Parking.UnitTests.Domain;

internal static class TestData
{
    public static readonly DateOnly Today = new(2026, 11, 7);
    public static readonly DateTime Now = new(2026, 11, 7, 10, 0, 0, DateTimeKind.Utc);

    public static Customer Customer() => Parking.Domain.Customers.Customer.Create("Maria Silva", "123456789LA123", new DateOnly(1990, 5, 20), Today);

    public static VehicleBrand Brand(int id = 1) => WithId(VehicleBrand.Create("Toyota"), id);

    public static Vehicle Vehicle(VehicleBrand? brand = null) => Parking.Domain.Vehicles.Vehicle.Create("LD-12-34-AB", brand ?? Brand(), null, "#FF0000");

    public static ParkingSpace Space(string code = "A1") => ParkingSpace.Create(code);

    /// <summary>Simulates an id assigned by the database.</summary>
    public static T WithId<T>(T entity, int id)
        where T : class
    {
        typeof(T).BaseType!.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance)!.SetValue(entity, id);
        return entity;
    }
}
