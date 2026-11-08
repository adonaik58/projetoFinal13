namespace Parking.Infrastructure.Persistence.Configurations;

/// <summary>Index names referenced outside the model (conflict translation, legacy ETL).</summary>
internal static class IndexNames
{
    public const string UserUsername = "ux_users_username";
    public const string SpaceCode = "ux_parking_spaces_code";
    public const string BrandName = "ux_vehicle_brands_name";
    public const string ModelBrandName = "ux_vehicle_models_brand_name";
    public const string TicketActiveSpace = "ux_tickets_active_space";
    public const string TicketActiveCustomer = "ux_tickets_active_customer";
    public const string TicketActiveVehicle = "ux_tickets_active_vehicle";
}
