using ADAM.Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using TUnit.Core.Interfaces;

namespace ADAM.IntegrationTests.TestConfiguration;

[ClassDataSource<TestWebAppFactory>(Shared = SharedType.PerClass)]
public class IntegrationTestBase : IAsyncDisposable
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected AppDbContext DbCtx;

    protected IntegrationTestBase(TestWebAppFactory factory)
    {
        Factory = factory;
    }

    [Before(Test)]
    public async Task InitializeAsync()
    {
        DbCtx = Factory.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
        await OnInitAsync();
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await OnDisposeAsync();
    }

    protected HttpClient GetHttpClient() => Factory.CreateClient(
        new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        }
    );

    protected virtual Task OnInitAsync() => Task.CompletedTask;
    protected virtual Task OnDisposeAsync() => Task.CompletedTask;
    public virtual Task LoadDependenciesAsync() => Task.CompletedTask;
}