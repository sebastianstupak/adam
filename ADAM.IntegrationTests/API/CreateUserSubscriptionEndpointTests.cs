using System.Net.Http.Json;
using ADAM.Application.Objects;
using ADAM.Domain.Models;
using ADAM.IntegrationTests.TestConfiguration;
using Microsoft.EntityFrameworkCore;

namespace ADAM.IntegrationTests.API;

[ClassDataSource<TestWebAppFactory>(Shared = SharedType.Keyed, Key = "Subscriptions")]
[Property("TestCategory", "Container")]
public class CreateUserSubscriptionEndpointTests(TestWebAppFactory factory) : IntegrationTestBase(factory)
{
    protected override Task OnInitAsync()
    {
        DbCtx.ChangeTracker.Clear();
        return Task.CompletedTask;
    }

    protected override async Task OnDisposeAsync() => await DbCtx.Database.EnsureDeletedAsync();

    [Test]
    public async Task CreateUserSubscription_WhenCalledWithValidData_CreatesSubscription()
    {
        var httpClient = GetHttpClient();

        var (response, dto) = await CreateUserSubscription(httpClient);

        await EnsureCreatedAsync((response, dto));
    }

    [Test, DependsOn(nameof(CreateUserSubscription_WhenCalledWithValidData_CreatesSubscription))]
    public async Task UpdateUserSubscription_WhenCalledWithValidData_UpdatesSubscription()
    {
        const string updateValue = "update";

        var httpClient = GetHttpClient();

        var (response, dto) = await CreateUserSubscription(httpClient);

        var subscriptionId = await EnsureCreatedAsync((response, dto));

        var updateDto = new UpdateUserSubscriptionDto { NewValue = updateValue };

        var updateResponse = await httpClient.PutAsJsonAsync($"api/v1/subscriptions/{subscriptionId}", updateDto);

        DbCtx.ChangeTracker.Clear(); // Clear change tracker to get DB data later

        await Assert.That(updateResponse).IsNotNull();
        await Assert.That(updateResponse.IsSuccessStatusCode).IsTrue();
        await Assert.That(DbCtx.Find<Subscription>(subscriptionId)!.Value == updateValue).IsTrue();
    }

    [Test, DependsOn(nameof(UpdateUserSubscription_WhenCalledWithValidData_UpdatesSubscription))]
    public async Task DeleteUserSubscription_WhenCalledWithValidData_DeletesSubscription()
    {
        var httpClient = GetHttpClient();

        var (response, dto) = await CreateUserSubscription(httpClient);

        var subscriptionId = await EnsureCreatedAsync((response, dto));

        var deleteResponse = await httpClient.DeleteAsync($"api/v1/subscriptions/{subscriptionId}");

        DbCtx.ChangeTracker.Clear(); // Clear change tracker to get DB data later

        await Assert.That(deleteResponse).IsNotNull();
        await Assert.That(deleteResponse.IsSuccessStatusCode).IsTrue();
        await Assert.That(DbCtx.Find<Subscription>(subscriptionId)).IsNull();
    }

    private async Task<(HttpResponseMessage response, CreateUserSubscriptionDto dto)> CreateUserSubscription(
        HttpClient client)
    {
        var dto = new CreateUserSubscriptionDto
        {
            UserGuid = Guid.NewGuid(),
            Type = (SubscriptionType)Random.Shared.Next(0, 2), // 0 or 1 
            Value = "test"
        };

        return (await client.PostAsJsonAsync("api/v1/subscription", dto), dto);
    }

    private async Task<long> EnsureCreatedAsync((HttpResponseMessage response, CreateUserSubscriptionDto dto) tuple)
    {
        var (response, dto) = tuple;

        await Assert.That(response).IsNotNull();
        await Assert.That(response.IsSuccessStatusCode).IsTrue();
        await Assert.That(DbCtx.Subscriptions.AnyAsync(s => s.User.Guid == dto.UserGuid)).IsTrue();

        return (await DbCtx.Subscriptions.FirstAsync()).Id;
    }
}