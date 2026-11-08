using System.Net;
using System.Net.Http.Json;

namespace Parking.IntegrationTests;

[Collection(ApiCollection.Name)]
public sealed class ParkingFlowTests(ParkingApiFactory factory)
{
    [Fact]
    public async Task Check_in_then_check_out_prices_the_stay_with_the_tariff_snapshot()
    {
        var admin = await factory.LoginAsync();
        var (brandId, modelId) = await admin.CreateBrandWithModelAsync();
        var spaceId = await admin.CreateSpaceAsync();
        var plate = TestHelpers.Plate();

        var opened = await admin.PostAsJsonAsync("api/v1/tickets", TestHelpers.CheckIn(spaceId, brandId, modelId, plate: plate))
            .ExpectAsync(HttpStatusCode.Created);
        var ticketId = opened.GetProperty("id").GetInt64();
        Assert.Equal("Open", opened.GetProperty("status").GetString());
        Assert.Equal(60, opened.GetProperty("freeMinutes").GetInt32());

        var board = await admin.GetAsync($"api/v1/spaces?plate={plate}").ExpectAsync(HttpStatusCode.OK);
        var occupied = Assert.Single(board.GetProperty("items").EnumerateArray());
        Assert.Equal("Occupied", occupied.GetProperty("status").GetString());
        Assert.Equal(ticketId, occupied.GetProperty("occupancy").GetProperty("ticketId").GetInt64());

        // 90 minutes later: (90 - 60 free) × 10 Kz = 300 Kz. A tariff change in between must not affect this ticket.
        factory.Clock.Advance(TimeSpan.FromMinutes(90));
        admin = await factory.LoginAsync();
        var settings = await admin.GetAsync("api/v1/settings").ExpectAsync(HttpStatusCode.OK);
        await admin.PutAsJsonAsync("api/v1/settings", new { ratePerMinute = 99, freeHours = 0, maxSpaces = settings.GetProperty("maxSpaces").GetInt32() })
            .ExpectAsync(HttpStatusCode.OK);

        var closed = await admin.PostAsync($"api/v1/tickets/{ticketId}/close", null).ExpectAsync(HttpStatusCode.OK);
        Assert.Equal("Closed", closed.GetProperty("status").GetString());
        Assert.Equal(90, closed.GetProperty("durationMinutes").GetInt32());
        Assert.Equal(300m, closed.GetProperty("amount").GetDecimal());

        var space = await admin.GetAsync("api/v1/spaces").ExpectAsync(HttpStatusCode.OK);
        Assert.Equal("Free", space.GetProperty("items").EnumerateArray().Single(s => s.GetProperty("id").GetInt32() == spaceId).GetProperty("status").GetString());

        await admin.PutAsJsonAsync("api/v1/settings", new { ratePerMinute = 10, freeHours = 1, maxSpaces = settings.GetProperty("maxSpaces").GetInt32() })
            .ExpectAsync(HttpStatusCode.OK);
        var again = await admin.PostAsync($"api/v1/tickets/{ticketId}/close", null).ExpectAsync(HttpStatusCode.Conflict);
        Assert.Equal("ticket.already_closed", again.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Concurrent_check_ins_for_the_same_space_admit_exactly_one()
    {
        var admin = await factory.LoginAsync();
        var (brandId, _) = await admin.CreateBrandWithModelAsync();
        var spaceId = await admin.CreateSpaceAsync();

        var attempts = await Task.WhenAll(Enumerable.Range(0, 6).Select(_ =>
            admin.PostAsJsonAsync("api/v1/tickets", TestHelpers.CheckIn(spaceId, brandId, null))));

        Assert.Equal(1, attempts.Count(r => r.StatusCode == HttpStatusCode.Created));
        Assert.All(attempts.Where(r => r.StatusCode != HttpStatusCode.Created), r => Assert.Equal(HttpStatusCode.Conflict, r.StatusCode));
    }

    [Fact]
    public async Task A_customer_or_vehicle_cannot_hold_two_open_tickets_even_concurrently()
    {
        var admin = await factory.LoginAsync();
        var (brandId, _) = await admin.CreateBrandWithModelAsync();
        var nationalId = TestHelpers.NationalId();
        var spaces = new[] { await admin.CreateSpaceAsync(), await admin.CreateSpaceAsync(), await admin.CreateSpaceAsync() };

        var attempts = await Task.WhenAll(spaces.Select(spaceId =>
            admin.PostAsJsonAsync("api/v1/tickets", TestHelpers.CheckIn(spaceId, brandId, null, nationalId: nationalId))));
        Assert.Equal(1, attempts.Count(r => r.StatusCode == HttpStatusCode.Created));

        var plate = TestHelpers.Plate();
        await admin.PostAsJsonAsync("api/v1/tickets", TestHelpers.CheckIn(await admin.CreateSpaceAsync(), brandId, null, plate: plate)).ExpectAsync(HttpStatusCode.Created);
        var problem = await admin.PostAsJsonAsync("api/v1/tickets", TestHelpers.CheckIn(await admin.CreateSpaceAsync(), brandId, null, plate: plate))
            .ExpectAsync(HttpStatusCode.Conflict);
        Assert.Equal("ticket.vehicle_has_open_ticket", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Idempotency_key_replays_the_first_response_and_rejects_a_different_payload()
    {
        var admin = await factory.LoginAsync();
        var (brandId, modelId) = await admin.CreateBrandWithModelAsync();
        var key = Guid.NewGuid().ToString();
        var body = TestHelpers.CheckIn(await admin.CreateSpaceAsync(), brandId, modelId);

        using var first = await Send(admin, body, key);
        using var second = await Send(admin, body, key);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
        Assert.Equal("true", second.Headers.GetValues("Idempotent-Replayed").Single());
        Assert.Equal(first.Headers.Location, second.Headers.Location);
        Assert.Equal((await first.ReadJsonAsync()).GetProperty("id").GetInt64(), (await second.ReadJsonAsync()).GetProperty("id").GetInt64());

        using var reused = await Send(admin, TestHelpers.CheckIn(await admin.CreateSpaceAsync(), brandId, modelId), key);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, reused.StatusCode);

        static Task<HttpResponseMessage> Send(HttpClient client, object payload, string idempotencyKey)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/tickets") { Content = JsonContent.Create(payload) };
            request.Headers.Add("Idempotency-Key", idempotencyKey);
            return client.SendAsync(request);
        }
    }

    [Fact]
    public async Task Invalid_check_in_reports_every_field_using_json_paths()
    {
        var admin = await factory.LoginAsync();

        var problem = await admin.PostAsJsonAsync("api/v1/tickets", new
        {
            spaceId = 0,
            customer = new { fullName = "", nationalId = "123", birthDate = "2999-01-01" },
            vehicle = new { plate = "?", brandId = 0, color = "blue" },
        })
            .ExpectAsync(HttpStatusCode.BadRequest);

        var fields = problem.GetProperty("errors").EnumerateObject().Select(p => p.Name).ToHashSet();
        Assert.Superset(new HashSet<string> { "spaceId", "customer.fullName", "customer.nationalId", "customer.birthDate", "vehicle.plate", "vehicle.brandId", "vehicle.color" }, fields);
    }

    [Fact]
    public async Task Returning_customers_are_found_by_national_id_with_their_last_vehicle()
    {
        var admin = await factory.LoginAsync();
        var (brandId, modelId) = await admin.CreateBrandWithModelAsync();
        var nationalId = TestHelpers.NationalId();
        var plate = TestHelpers.Plate();
        await admin.PostAsJsonAsync("api/v1/tickets", TestHelpers.CheckIn(await admin.CreateSpaceAsync(), brandId, modelId, nationalId, plate))
            .ExpectAsync(HttpStatusCode.Created);

        var customer = await admin.GetAsync($"api/v1/customers/lookup?nationalId={nationalId}").ExpectAsync(HttpStatusCode.OK);

        Assert.Equal("Cliente Teste", customer.GetProperty("fullName").GetString());
        Assert.Equal(plate, customer.GetProperty("lastVehicle").GetProperty("plate").GetString());
        await admin.GetAsync($"api/v1/customers/lookup?nationalId={TestHelpers.NationalId()}").ExpectAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Ticket_history_supports_filters_sorting_and_paging()
    {
        var admin = await factory.LoginAsync();
        var (brandId, modelId) = await admin.CreateBrandWithModelAsync();
        for (var i = 0; i < 3; i++)
        {
            var ticket = await admin.PostAsJsonAsync("api/v1/tickets", TestHelpers.CheckIn(await admin.CreateSpaceAsync(), brandId, modelId))
                .ExpectAsync(HttpStatusCode.Created);
            await admin.PostAsync($"api/v1/tickets/{ticket.GetProperty("id").GetInt64()}/close", null).ExpectAsync(HttpStatusCode.OK);
        }

        var page = await admin.GetAsync($"api/v1/tickets?status=Closed&brandId={brandId}&sortBy=entryAt&sortDir=asc&page=1&pageSize=2")
            .ExpectAsync(HttpStatusCode.OK);

        Assert.Equal(3, page.GetProperty("totalCount").GetInt32());
        Assert.Equal(2, page.GetProperty("totalPages").GetInt32());
        var items = page.GetProperty("items").EnumerateArray().ToList();
        Assert.Equal(2, items.Count);
        Assert.True(items[0].GetProperty("id").GetInt64() < items[1].GetProperty("id").GetInt64());

        await admin.GetAsync("api/v1/tickets?sortBy=password").ExpectAsync(HttpStatusCode.BadRequest);
        await admin.GetAsync("api/v1/tickets?from=yesterday").ExpectAsync(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Space_capacity_and_history_rules_are_enforced()
    {
        var admin = await factory.LoginAsync();
        await admin.CreateSpaceAsync();
        await admin.CreateSpaceAsync();
        var settings = await admin.GetAsync("api/v1/settings").ExpectAsync(HttpStatusCode.OK);
        var originalCapacity = settings.GetProperty("maxSpaces").GetInt32();
        var total = (await admin.GetAsync("api/v1/spaces").ExpectAsync(HttpStatusCode.OK)).GetProperty("summary").GetProperty("total").GetInt32();
        Assert.True(total >= 2);

        try
        {
            await admin.PutAsJsonAsync("api/v1/settings", new { ratePerMinute = 10, freeHours = 1, maxSpaces = total + 1 }).ExpectAsync(HttpStatusCode.OK);
            var problem = await admin.PostAsJsonAsync("api/v1/spaces/bulk", new { sections = new[] { "ZZ" }, fromNumber = 1, toNumber = 5 })
                .ExpectAsync(HttpStatusCode.Conflict);
            Assert.Equal("space.capacity_reached", problem.GetProperty("code").GetString());

            var shrink = await admin.PutAsJsonAsync("api/v1/settings", new { ratePerMinute = 10, freeHours = 1, maxSpaces = total - 1 })
                .ExpectAsync(HttpStatusCode.Conflict);
            Assert.Equal("settings.capacity_below_existing", shrink.GetProperty("code").GetString());
        }
        finally
        {
            await admin.PutAsJsonAsync("api/v1/settings", new { ratePerMinute = 10, freeHours = 1, maxSpaces = originalCapacity })
                .ExpectAsync(HttpStatusCode.OK);
        }

        var (brandId, _) = await admin.CreateBrandWithModelAsync();
        var used = await admin.CreateSpaceAsync();
        await admin.PostAsJsonAsync("api/v1/tickets", TestHelpers.CheckIn(used, brandId, null)).ExpectAsync(HttpStatusCode.Created);
        Assert.Equal("space.occupied", (await admin.PatchAsJsonAsync($"api/v1/spaces/{used}/status", new { status = "Maintenance" })
            .ExpectAsync(HttpStatusCode.Conflict)).GetProperty("code").GetString());
        await admin.DeleteAsync($"api/v1/spaces/{used}").ExpectAsync(HttpStatusCode.Conflict);

        var unused = await admin.CreateSpaceAsync();
        await admin.PatchAsJsonAsync($"api/v1/spaces/{unused}/status", new { status = "Maintenance" }).ExpectAsync(HttpStatusCode.OK);
        await admin.DeleteAsync($"api/v1/spaces/{unused}").ExpectAsync(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Dashboard_reflects_new_activity_immediately_after_cache_invalidation()
    {
        var admin = await factory.LoginAsync();
        var before = await admin.GetAsync("api/v1/dashboard/summary?period=all").ExpectAsync(HttpStatusCode.OK);
        var (brandId, _) = await admin.CreateBrandWithModelAsync();

        await admin.PostAsJsonAsync("api/v1/tickets", TestHelpers.CheckIn(await admin.CreateSpaceAsync(), brandId, null)).ExpectAsync(HttpStatusCode.Created);

        var after = await admin.GetAsync("api/v1/dashboard/summary?period=all").ExpectAsync(HttpStatusCode.OK);
        Assert.Equal(before.GetProperty("entries").GetInt32() + 1, after.GetProperty("entries").GetInt32());
        Assert.Equal(before.GetProperty("openTickets").GetInt32() + 1, after.GetProperty("openTickets").GetInt32());

        var series = await admin.GetAsync("api/v1/dashboard/revenue-series?days=7").ExpectAsync(HttpStatusCode.OK);
        Assert.Equal(7, series.GetProperty("points").GetArrayLength());
        await admin.GetAsync("api/v1/dashboard/summary?period=week").ExpectAsync(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Catalog_rejects_duplicates_and_unknown_brands()
    {
        var admin = await factory.LoginAsync();
        var (brandId, _) = await admin.CreateBrandWithModelAsync();
        var brands = await admin.GetAsync("api/v1/catalog/brands").ExpectAsync(HttpStatusCode.OK);
        var name = brands.EnumerateArray().Single(b => b.GetProperty("id").GetInt32() == brandId).GetProperty("name").GetString();

        await admin.PostAsJsonAsync("api/v1/catalog/brands", new { name }).ExpectAsync(HttpStatusCode.Conflict);
        await admin.PostAsJsonAsync($"api/v1/catalog/brands/{brandId}/models", new { name = "Modelo X" }).ExpectAsync(HttpStatusCode.Conflict);
        await admin.GetAsync("api/v1/catalog/brands/999999/models").ExpectAsync(HttpStatusCode.NotFound);
    }
}
