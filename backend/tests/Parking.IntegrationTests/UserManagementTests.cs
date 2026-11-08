using System.Net;
using System.Net.Http.Json;

namespace Parking.IntegrationTests;

[Collection(ApiCollection.Name)]
public sealed class UserManagementTests(ParkingApiFactory factory)
{
    private static async Task<(int Id, string Username)> CreateOperatorAsync(HttpClient admin, string password = "Operador123")
    {
        var username = $"operador{TestHelpers.Next()}";
        var user = await admin.PostAsJsonAsync("api/v1/users", new { username, fullName = "Operador Teste", role = "Operator", password })
            .ExpectAsync(HttpStatusCode.Created);
        return (user.GetProperty("id").GetInt32(), username);
    }

    [Fact]
    public async Task Operators_only_reach_the_endpoints_their_role_allows()
    {
        var admin = await factory.LoginAsync();
        var (_, username) = await CreateOperatorAsync(admin);
        var operatorClient = await factory.LoginAsync(username, "Operador123");

        await operatorClient.GetAsync("api/v1/spaces").ExpectAsync(HttpStatusCode.OK);
        await operatorClient.GetAsync("api/v1/tickets").ExpectAsync(HttpStatusCode.OK);
        await operatorClient.GetAsync("api/v1/users").ExpectAsync(HttpStatusCode.Forbidden);
        await operatorClient.GetAsync("api/v1/dashboard/summary").ExpectAsync(HttpStatusCode.Forbidden);
        await operatorClient.PutAsJsonAsync("api/v1/settings", new { ratePerMinute = 1, freeHours = 0, maxSpaces = 10 }).ExpectAsync(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Duplicate_usernames_are_rejected_case_insensitively()
    {
        var admin = await factory.LoginAsync();
        var (_, username) = await CreateOperatorAsync(admin);

        var problem = await admin.PostAsJsonAsync("api/v1/users",
                new { username = username.ToUpperInvariant(), fullName = "Outro", role = "Operator", password = "Operador123" })
            .ExpectAsync(HttpStatusCode.Conflict);
        Assert.Equal("user.username_taken", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Weak_passwords_return_field_errors()
    {
        var admin = await factory.LoginAsync();

        var problem = await admin.PostAsJsonAsync("api/v1/users", new { username = "fraco", fullName = "Fraco", role = "Operator", password = "abc" })
            .ExpectAsync(HttpStatusCode.BadRequest);
        Assert.True(problem.GetProperty("errors").TryGetProperty("password", out _));
    }

    [Fact]
    public async Task Admins_cannot_deactivate_themselves()
    {
        var admin = await factory.LoginAsync();
        var me = await admin.GetAsync("api/v1/auth/me").ExpectAsync(HttpStatusCode.OK);

        var problem = await admin.PostAsync($"api/v1/users/{me.GetProperty("id").GetInt32()}/deactivate", null).ExpectAsync(HttpStatusCode.Forbidden);
        Assert.Equal("user.cannot_deactivate_self", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Deactivated_users_lose_access_and_can_be_reactivated()
    {
        var admin = await factory.LoginAsync();
        var (id, username) = await CreateOperatorAsync(admin);
        var operatorClient = await factory.LoginAsync(username, "Operador123");

        await admin.PostAsync($"api/v1/users/{id}/deactivate", null).ExpectAsync(HttpStatusCode.NoContent);

        await operatorClient.PostAsync("api/v1/auth/refresh", null).ExpectAsync(HttpStatusCode.Unauthorized);
        await factory.CreateApiClient().PostAsJsonAsync("api/v1/auth/login", new { username, password = "Operador123" }).ExpectAsync(HttpStatusCode.Unauthorized);

        await admin.PostAsync($"api/v1/users/{id}/activate", null).ExpectAsync(HttpStatusCode.NoContent);
        await factory.LoginAsync(username, "Operador123");
    }

    [Fact]
    public async Task Admin_can_reset_a_password_and_users_can_change_their_own()
    {
        var admin = await factory.LoginAsync();
        var (id, username) = await CreateOperatorAsync(admin);

        await admin.PostAsJsonAsync($"api/v1/users/{id}/reset-password", new { newPassword = "Reposta123" }).ExpectAsync(HttpStatusCode.NoContent);
        var self = await factory.LoginAsync(username, "Reposta123");

        var wrong = await self.PostAsJsonAsync("api/v1/auth/change-password", new { currentPassword = "Errada123", newPassword = "NovaSenha123" })
            .ExpectAsync(HttpStatusCode.BadRequest);
        Assert.True(wrong.GetProperty("errors").TryGetProperty("currentPassword", out _));

        await self.PostAsJsonAsync("api/v1/auth/change-password", new { currentPassword = "Reposta123", newPassword = "NovaSenha123" })
            .ExpectAsync(HttpStatusCode.NoContent);
        await factory.LoginAsync(username, "NovaSenha123");
    }

    [Fact]
    public async Task User_list_is_paged_and_reports_active_sessions()
    {
        var admin = await factory.LoginAsync();
        await CreateOperatorAsync(admin);

        var page = await admin.GetAsync("api/v1/users?page=1&pageSize=2&role=Operator").ExpectAsync(HttpStatusCode.OK);

        Assert.Equal(2, page.GetProperty("pageSize").GetInt32());
        Assert.True(page.GetProperty("items").GetArrayLength() <= 2);
        Assert.All(page.GetProperty("items").EnumerateArray(), u => Assert.Equal("Operator", u.GetProperty("role").GetString()));
        await admin.GetAsync("api/v1/users?role=Chief").ExpectAsync(HttpStatusCode.BadRequest);
    }
}
