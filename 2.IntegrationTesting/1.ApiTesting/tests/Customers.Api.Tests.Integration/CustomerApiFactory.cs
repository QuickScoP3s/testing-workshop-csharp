using System.Data.Common;
using Bogus;
using Customers.Api.Database;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Net.Http.Headers;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using Xunit;

namespace Customers.Api.Tests.Integration;

public class CustomerApiFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
{
    private DbConnection _dbConnection = null!;
    private Respawner _respawner = null!;

    public GitHubApiServer GitHubApiServer { get; } = new();

    private readonly PostgreSqlContainer _dbContainer =
        new PostgreSqlBuilder()
            .WithDockerEndpoint("unix:///Users/nickchapsas/.docker/run/docker.sock")
            .WithDatabase("mydb")
            .WithUsername("workshop")
            .WithPassword("changeme")
            .Build();

    public HttpClient HttpClient { get; private set; } = null!;
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(x =>
        {
            x.ClearProviders();
        });

        builder.ConfigureTestServices(x =>
        {
            x.RemoveAll(typeof(IDbConnectionFactory));
            x.AddSingleton<IDbConnectionFactory>(
                new NpgsqlConnectionFactory(_dbContainer.GetConnectionString()));
            
            x.AddHttpClient("GitHub", httpClient =>
            {
                httpClient.BaseAddress = new Uri(GitHubApiServer.Url);
                httpClient.DefaultRequestHeaders.Add(
                    HeaderNames.Accept, "application/vnd.github.v3+json");
                httpClient.DefaultRequestHeaders.Add(
                    HeaderNames.UserAgent, $"Workshop-{Environment.MachineName}");
            });
        });
    }

    public async Task InitializeAsync()
    {
        // Once at the start for everything
        Randomizer.Seed = new Random(1111);
        await _dbContainer.StartAsync();
        GitHubApiServer.Start();
        
        HttpClient = CreateClient();
        
        _dbConnection =
            new NpgsqlConnection(_dbContainer.GetConnectionString());
        await _dbConnection.OpenAsync();
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new []{ "public" }
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }

    public new async Task DisposeAsync()
    {
        // Once at the stop for everything
        GitHubApiServer.Dispose();
        await _dbContainer.StopAsync();
    }
}
