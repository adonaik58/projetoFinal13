using System.Globalization;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Parking.Application.Abstractions;
using Parking.Application.Abstractions.Messaging;
using Parking.Application.Common.Exceptions;
using Parking.Domain.Vehicles;

namespace Parking.Application.Features.Catalog;

public sealed record BrandDto(int Id, string Name);

public sealed record ModelDto(int Id, int BrandId, string Name);

internal static class CatalogCache
{
    public static readonly CacheEntrySettings Entry = new(TimeSpan.FromHours(6));
}

public sealed record ListBrandsQuery : IQuery<IReadOnlyList<BrandDto>>;

internal sealed class ListBrandsHandler(IAppDbContext db, ICacheService cache) : IQueryHandler<ListBrandsQuery, IReadOnlyList<BrandDto>>
{
    public async Task<IReadOnlyList<BrandDto>> Handle(ListBrandsQuery request, CancellationToken cancellationToken) =>
        await cache.GetOrCreateAsync<IReadOnlyList<BrandDto>>(
            CacheKeys.Brands,
            async ct => await db.VehicleBrands.AsNoTracking().OrderBy(b => b.Name).Select(b => new BrandDto(b.Id, b.Name)).ToListAsync(ct),
            CatalogCache.Entry,
            cancellationToken);
}

public sealed record ListModelsQuery(int BrandId) : IQuery<IReadOnlyList<ModelDto>>;

internal sealed class ListModelsHandler(IAppDbContext db, ICacheService cache) : IQueryHandler<ListModelsQuery, IReadOnlyList<ModelDto>>
{
    public async Task<IReadOnlyList<ModelDto>> Handle(ListModelsQuery request, CancellationToken cancellationToken)
    {
        if (!await db.VehicleBrands.AnyAsync(b => b.Id == request.BrandId, cancellationToken))
        {
            throw new NotFoundException("catalog.brand_not_found", "Marca não encontrada.");
        }

        return await cache.GetOrCreateAsync<IReadOnlyList<ModelDto>>(
            CacheKeys.Models(request.BrandId),
            async ct => await db.VehicleModels.AsNoTracking()
                .Where(m => m.BrandId == request.BrandId)
                .OrderBy(m => m.Name)
                .Select(m => new ModelDto(m.Id, m.BrandId, m.Name))
                .ToListAsync(ct),
            CatalogCache.Entry,
            cancellationToken);
    }
}

public sealed record CreateBrandCommand(string Name) : ICommand<BrandDto>;

internal sealed class CreateBrandValidator : AbstractValidator<CreateBrandCommand>
{
    public CreateBrandValidator() =>
        RuleFor(x => x.Name).NotEmpty().WithMessage("Indique o nome da marca.").MaximumLength(VehicleBrand.NameMaxLength);
}

internal sealed class CreateBrandHandler(IAppDbContext db, ICacheService cache, IAuditLogger audit) : ICommandHandler<CreateBrandCommand, BrandDto>
{
    public async Task<BrandDto> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        if (await db.VehicleBrands.AnyAsync(b => b.Name == name, cancellationToken))
        {
            throw new ConflictException("catalog.brand_exists", "Já existe uma marca com este nome.");
        }

        var brand = VehicleBrand.Create(name);
        db.VehicleBrands.Add(brand);
        audit.Record("catalog.brand_created", nameof(VehicleBrand), brand.Name);
        await db.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync([CacheKeys.Brands], cancellationToken);
        return new BrandDto(brand.Id, brand.Name);
    }
}

public sealed record CreateModelCommand(int BrandId, string Name) : ICommand<ModelDto>;

internal sealed class CreateModelValidator : AbstractValidator<CreateModelCommand>
{
    public CreateModelValidator() =>
        RuleFor(x => x.Name).NotEmpty().WithMessage("Indique o nome do modelo.").MaximumLength(VehicleModel.NameMaxLength);
}

internal sealed class CreateModelHandler(IAppDbContext db, ICacheService cache, IAuditLogger audit) : ICommandHandler<CreateModelCommand, ModelDto>
{
    public async Task<ModelDto> Handle(CreateModelCommand request, CancellationToken cancellationToken)
    {
        if (!await db.VehicleBrands.AnyAsync(b => b.Id == request.BrandId, cancellationToken))
        {
            throw new NotFoundException("catalog.brand_not_found", "Marca não encontrada.");
        }

        var name = request.Name.Trim();
        if (await db.VehicleModels.AnyAsync(m => m.BrandId == request.BrandId && m.Name == name, cancellationToken))
        {
            throw new ConflictException("catalog.model_exists", "Esta marca já tem um modelo com este nome.");
        }

        var model = VehicleModel.Create(request.BrandId, name);
        db.VehicleModels.Add(model);
        audit.Record("catalog.model_created", nameof(VehicleModel), model.Name,
            new { brandId = request.BrandId.ToString(CultureInfo.InvariantCulture) });
        await db.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync([CacheKeys.Models(request.BrandId)], cancellationToken);
        return new ModelDto(model.Id, model.BrandId, model.Name);
    }
}
