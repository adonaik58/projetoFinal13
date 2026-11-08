using FluentValidation;
using NSubstitute;
using Parking.Application.Abstractions;
using Parking.Application.Features.Settings;
using Parking.Application.Features.Spaces;
using Parking.Application.Features.Tickets;
using Parking.Application.Features.Users;
using Parking.Domain.Users;

namespace Parking.UnitTests.Application;

public sealed class ValidatorTests
{
    private static readonly DateOnly Today = new(2026, 11, 7);

    private static OpenTicketValidator TicketValidator()
    {
        var clock = Substitute.For<IBusinessClock>();
        clock.Today.Returns(Today);
        return new OpenTicketValidator(clock);
    }

    private static OpenTicketCommand ValidTicket() => new(
        1,
        new CustomerInput("Maria Silva", "123456789LA123", new DateOnly(1990, 1, 1)),
        new VehicleInput("LD-12-34-AB", 3, null, "#00FF00"));

    [Fact]
    public void Open_ticket_accepts_a_complete_request() =>
        Assert.True(TicketValidator().Validate(ValidTicket()).IsValid);

    [Fact]
    public void Open_ticket_reports_each_invalid_field_with_its_path()
    {
        var command = ValidTicket() with
        {
            Customer = new CustomerInput("", "BAD", Today.AddDays(1)),
            Vehicle = new VehicleInput("!", 0, null, "red"),
        };

        var fields = TicketValidator().Validate(command).Errors.Select(e => e.PropertyName).ToHashSet();

        Assert.Superset(
            new HashSet<string> { "Customer.FullName", "Customer.NationalId", "Customer.BirthDate", "Vehicle.Plate", "Vehicle.BrandId", "Vehicle.Color" },
            fields);
    }

    [Theory]
    [InlineData("short1", false)]
    [InlineData("onlyletters", false)]
    [InlineData("12345678", false)]
    [InlineData("Valid123", true)]
    public void New_passwords_need_length_letters_and_digits(string password, bool valid) =>
        Assert.Equal(valid, new CreateUserValidator().Validate(new CreateUserCommand("operador1", "Operador Um", Role.Operator, password)).IsValid);

    [Theory]
    [InlineData("ab", false)]
    [InlineData("joão", false)]
    [InlineData("op.erador_1-x", true)]
    public void Usernames_are_restricted_to_safe_characters(string username, bool valid) =>
        Assert.Equal(valid, new CreateUserValidator().Validate(new CreateUserCommand(username, "Nome", Role.Operator, "Valid123")).IsValid);

    [Fact]
    public void Bulk_space_creation_limits_the_batch_size()
    {
        var validator = new BulkCreateSpacesValidator();

        Assert.True(validator.Validate(new BulkCreateSpacesCommand(["A", "B"], 1, 10)).IsValid);
        Assert.False(validator.Validate(new BulkCreateSpacesCommand(["A"], 10, 1)).IsValid);
        Assert.False(validator.Validate(new BulkCreateSpacesCommand(["A1"], 1, 10)).IsValid);
        Assert.False(validator.Validate(new BulkCreateSpacesCommand(["A", "B", "C"], 1, 9999)).IsValid);
    }

    [Fact]
    public void Settings_accept_at_most_two_decimal_places()
    {
        var validator = new UpdateSettingsValidator();

        Assert.True(validator.Validate(new UpdateSettingsCommand(12.5m, 1, 100)).IsValid);
        Assert.False(validator.Validate(new UpdateSettingsCommand(12.555m, 1, 100)).IsValid);
        Assert.False(validator.Validate(new UpdateSettingsCommand(0m, 1, 100)).IsValid);
    }

    [Fact]
    public void Validators_are_discoverable_by_the_pipeline() =>
        Assert.IsAssignableFrom<IValidator<OpenTicketCommand>>(TicketValidator());
}
