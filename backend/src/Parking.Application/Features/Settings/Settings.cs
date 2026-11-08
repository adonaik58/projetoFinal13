using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Parking.Application.Abstractions;
using Parking.Application.Abstractions.Messaging;
using Parking.Application.Common.Exceptions;
using Parking.Domain.Settings;
using Parking.Domain.Tickets;

namespace Parking.Application.Features.Settings;

public sealed record SettingsDto(decimal RatePerMinute, int FreeHours, int MaxSpaces, DateTime UpdatedAt, string? UpdatedBy);

public sealed record TariffExampleDto(int Minutes, decimal Amount);

public sealed record TariffDto(decimal RatePerMinute, int FreeHours, IReadOnlyList<TariffExampleDto> Examples);

internal static class SettingsReader
{
    private static readonly CacheEntrySettings CacheSettings = new(TimeSpan.FromHours(1));

    public static ValueTask<SettingsDto> GetAsync(IAppDbContext db, ICacheService cache, CancellationToken cancellationToken) =>
        cache.GetOrCreateAsync(
            CacheKeys.Settings,
            async ct =>
                await db.ParkingSettings.AsNoTracking()
                    .Where(s => s.Id == ParkingSettings.SingletonId)
                    .Select(s => new SettingsDto(
                        s.RatePerMinute,
                        s.FreeHours,
                        s.MaxSpaces,
                        s.UpdatedAt,
                        db.Users.Where(u => u.Id == s.UpdatedByUserId).Select(u => u.FullName).FirstOrDefault()))
                    .SingleOrDefaultAsync(ct)
                ?? throw new NotFoundException("settings.not_found", "A configuração do parque não foi inicializada."),
            CacheSettings,
            cancellationToken);
}

public sealed record GetSettingsQuery : IQuery<SettingsDto>;

internal sealed class GetSettingsHandler(IAppDbContext db, ICacheService cache) : IQueryHandler<GetSettingsQuery, SettingsDto>
{
    public async Task<SettingsDto> Handle(GetSettingsQuery request, CancellationToken cancellationToken) =>
        await SettingsReader.GetAsync(db, cache, cancellationToken);
}

public sealed record GetTariffQuery : IQuery<TariffDto>;

internal sealed class GetTariffHandler(IAppDbContext db, ICacheService cache) : IQueryHandler<GetTariffQuery, TariffDto>
{
    private static readonly int[] ExampleMinutes = [15, 30, 60, 120, 180, 240, 480, 720, 1440];

    public async Task<TariffDto> Handle(GetTariffQuery request, CancellationToken cancellationToken)
    {
        var settings = await SettingsReader.GetAsync(db, cache, cancellationToken);
        var freeMinutes = settings.FreeHours * 60;
        var durations = freeMinutes > 0 ? ExampleMinutes.Append(freeMinutes).Append(freeMinutes + 60) : ExampleMinutes;
        var examples = durations.Distinct().Order()
            .Select(minutes => new TariffExampleDto(minutes, PricingPolicy.Calculate(minutes, settings.RatePerMinute, freeMinutes)))
            .ToList();
        return new TariffDto(settings.RatePerMinute, settings.FreeHours, examples);
    }
}

public sealed record UpdateSettingsCommand(decimal RatePerMinute, int FreeHours, int MaxSpaces) : ICommand<SettingsDto>;

internal sealed class UpdateSettingsValidator : AbstractValidator<UpdateSettingsCommand>
{
    public UpdateSettingsValidator()
    {
        RuleFor(x => x.RatePerMinute).GreaterThan(0).WithMessage("A tarifa por minuto deve ser superior a zero.")
            .LessThanOrEqualTo(ParkingSettings.MaxRatePerMinute)
            .PrecisionScale(12, 2, ignoreTrailingZeros: true).WithMessage("A tarifa admite no máximo 2 casas decimais.");
        RuleFor(x => x.FreeHours).InclusiveBetween(0, ParkingSettings.MaxFreeHours)
            .WithMessage($"As horas grátis devem estar entre 0 e {ParkingSettings.MaxFreeHours}.");
        RuleFor(x => x.MaxSpaces).InclusiveBetween(1, ParkingSettings.MaxCapacity)
            .WithMessage($"A capacidade deve estar entre 1 e {ParkingSettings.MaxCapacity}.");
    }
}

internal sealed class UpdateSettingsHandler(
    IAppDbContext db, ICacheService cache, ICurrentUser currentUser, IAuditLogger audit, IBusinessClock clock)
    : ICommandHandler<UpdateSettingsCommand, SettingsDto>
{
    public async Task<SettingsDto> Handle(UpdateSettingsCommand request, CancellationToken cancellationToken)
    {
        var settings = await db.ParkingSettings.SingleOrDefaultAsync(s => s.Id == ParkingSettings.SingletonId, cancellationToken)
            ?? throw new NotFoundException("settings.not_found", "A configuração do parque não foi inicializada.");
        var spaceCount = await db.ParkingSpaces.CountAsync(cancellationToken);

        var before = new { settings.RatePerMinute, settings.FreeHours, settings.MaxSpaces };
        settings.Update(request.RatePerMinute, request.FreeHours, request.MaxSpaces, spaceCount, currentUser.RequiredUserId, clock.UtcNow);
        audit.Record("settings.updated", nameof(ParkingSettings), null, new { before, after = request });
        await db.SaveChangesAsync(cancellationToken);

        await cache.RemoveAsync([CacheKeys.Settings], cancellationToken);
        return await SettingsReader.GetAsync(db, cache, cancellationToken);
    }
}
