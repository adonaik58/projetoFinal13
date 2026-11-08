using Parking.Domain.Common;
using Parking.Domain.Customers;
using Parking.Domain.Settings;
using Parking.Domain.Spaces;
using Parking.Domain.Users;
using Parking.Domain.Vehicles;

namespace Parking.UnitTests.Domain;

public sealed class EntityRulesTests
{
    [Theory]
    [InlineData("123456789LA123", true)]
    [InlineData("123456789la123", true)] // normalized to upper case
    [InlineData("12345678LA123", false)]
    [InlineData("123456789L1123", false)]
    [InlineData("", false)]
    public void National_id_follows_the_angolan_format(string value, bool valid) =>
        Assert.Equal(valid, Customer.IsValidNationalId(Customer.NormalizeNationalId(value)));

    [Fact]
    public void Customer_age_accounts_for_birthdays_not_yet_reached()
    {
        Assert.Equal(36, Customer.AgeOn(new DateOnly(1990, 11, 7), TestData.Today));
        Assert.Equal(35, Customer.AgeOn(new DateOnly(1990, 11, 8), TestData.Today));
    }

    [Fact]
    public void Customer_rejects_a_birth_date_in_the_future() =>
        Assert.Equal("customer.invalid_birth_date", Assert.Throws<DomainException>(() =>
            Customer.Create("Ana", "123456789LA123", TestData.Today.AddDays(1), TestData.Today)).Code);

    [Theory]
    [InlineData("a1", "A", 1)]
    [InlineData(" ab12 ", "AB", 12)]
    [InlineData("ZZZ9999", "ZZZ", 9999)]
    public void Space_codes_are_parsed_into_section_and_number(string code, string section, int number)
    {
        var space = ParkingSpace.Create(code);

        Assert.Equal(section, space.Section);
        Assert.Equal(number, space.Number);
        Assert.Equal($"{section}{number}", space.Code);
    }

    [Theory]
    [InlineData("1A")]
    [InlineData("A0")]
    [InlineData("ABCD1")]
    [InlineData("A10000")]
    public void Invalid_space_codes_are_rejected(string code) =>
        Assert.Throws<DomainException>(() => ParkingSpace.Create(code));

    [Fact]
    public void Occupied_spaces_cannot_go_to_maintenance()
    {
        var space = ParkingSpace.Create("B2");
        space.Occupy();

        Assert.Equal("space.occupied", Assert.Throws<DomainException>(space.SetMaintenance).Code);
    }

    [Theory]
    [InlineData("  ld-12-34  ab ", "LD-12-34 AB", true)]
    [InlineData("A", "A", false)]
    [InlineData("LD_12", "LD_12", false)]
    public void Plates_are_normalized_and_validated(string input, string normalized, bool valid)
    {
        Assert.Equal(normalized, Vehicle.NormalizePlate(input));
        Assert.Equal(valid, Vehicle.IsValidPlate(Vehicle.NormalizePlate(input)));
    }

    [Fact]
    public void Vehicle_model_must_belong_to_the_brand()
    {
        var model = TestData.WithId(VehicleModel.Create(brandId: 99, "Corolla"), 5);

        Assert.Equal("vehicle.model_brand_mismatch", Assert.Throws<DomainException>(() =>
            Vehicle.Create("LD-12-34-AB", TestData.Brand(1), model, "#000000")).Code);
    }

    [Fact]
    public void Settings_capacity_cannot_drop_below_existing_spaces()
    {
        var settings = ParkingSettings.Create(10m, 0, 100, TestData.Now);

        Assert.Equal("settings.capacity_below_existing", Assert.Throws<DomainException>(() =>
            settings.Update(10m, 0, 50, currentSpaceCount: 60, updatedByUserId: 1, TestData.Now)).Code);
    }

    [Theory]
    [InlineData(0, 1, 10)]
    [InlineData(10.123, 1, 10)]
    [InlineData(10, -1, 10)]
    [InlineData(10, 1, 0)]
    public void Settings_reject_invalid_tariffs(decimal rate, int freeHours, int maxSpaces) =>
        Assert.Throws<DomainException>(() => ParkingSettings.Create(rate, freeHours, maxSpaces, TestData.Now));

    [Fact]
    public void Permissions_follow_the_role_matrix()
    {
        Assert.Equal(Permissions.All, Permissions.ForRole(Role.Admin));
        Assert.Contains(Permissions.TicketsOpen, Permissions.ForRole(Role.Operator));
        Assert.DoesNotContain(Permissions.UsersManage, Permissions.ForRole(Role.Operator));
        Assert.DoesNotContain(Permissions.DashboardView, Permissions.ForRole(Role.Operator));
        Assert.Equal([Role.Admin], Permissions.RolesWith(Permissions.SettingsManage));
        Assert.Equal([Role.Operator, Role.Admin], Permissions.RolesWith(Permissions.SpacesView));
    }

    [Fact]
    public void Refresh_tokens_expire_and_revocation_is_idempotent()
    {
        var token = RefreshToken.Issue(1, new string('a', 64), Guid.NewGuid(), TestData.Now, TimeSpan.FromDays(7), "10.0.0.1");

        Assert.True(token.IsActive(TestData.Now.AddDays(6)));
        Assert.False(token.IsActive(TestData.Now.AddDays(7)));

        token.Revoke(TestData.Now, "rotated", "next");
        token.Revoke(TestData.Now.AddMinutes(1), "logout");
        Assert.Equal("rotated", token.RevokedReason);
        Assert.False(token.IsActive(TestData.Now));
    }
}
