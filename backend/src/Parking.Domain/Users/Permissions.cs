namespace Parking.Domain.Users;

public static class Permissions
{
    public const string DashboardView = "dashboard.view";
    public const string SpacesView = "spaces.view";
    public const string SpacesManage = "spaces.manage";
    public const string TicketsView = "tickets.view";
    public const string TicketsOpen = "tickets.open";
    public const string TicketsClose = "tickets.close";
    public const string CatalogView = "catalog.view";
    public const string CatalogManage = "catalog.manage";
    public const string SettingsView = "settings.view";
    public const string SettingsManage = "settings.manage";
    public const string UsersManage = "users.manage";
    public const string AuditView = "audit.view";

    public static readonly IReadOnlyList<string> All =
    [
        DashboardView, SpacesView, SpacesManage, TicketsView, TicketsOpen, TicketsClose,
        CatalogView, CatalogManage, SettingsView, SettingsManage, UsersManage, AuditView,
    ];

    private static readonly IReadOnlyList<string> OperatorPermissions =
    [
        SpacesView, TicketsView, TicketsOpen, TicketsClose, CatalogView, SettingsView,
    ];

    public static IReadOnlyList<string> ForRole(Role role) => role switch
    {
        Role.Admin => All,
        Role.Operator => OperatorPermissions,
        _ => [],
    };

    /// <summary>Roles granted a permission; used to build authorization policies.</summary>
    public static IReadOnlyList<Role> RolesWith(string permission) =>
        [.. Enum.GetValues<Role>().Where(role => ForRole(role).Contains(permission))];
}
