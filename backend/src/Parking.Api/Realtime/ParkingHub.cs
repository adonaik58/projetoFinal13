using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Parking.Domain.Users;

namespace Parking.Api.Realtime;

/// <summary>
/// Server-to-client channel replacing periodic polling: the server sends <see cref="ChangedMethod"/> with a topic
/// ("parking" or "users") and clients refresh the affected views through the regular API.
/// </summary>
[Authorize]
public sealed class ParkingHub : Hub
{
    public const string Path = "/hubs/parking";
    public const string ChangedMethod = "changed";
    public const string UserManagersGroup = "users-managers";

    public override async Task OnConnectedAsync()
    {
        if (Permissions.RolesWith(Permissions.UsersManage).Any(role => Context.User?.IsInRole(role.ToString()) == true))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, UserManagersGroup);
        }

        await base.OnConnectedAsync();
    }
}
