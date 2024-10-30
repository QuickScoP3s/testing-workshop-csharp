using Xunit;
using Xunit.Abstractions;

namespace Customers.Api.Tests.Integration;

public class LifecycleTests : IAsyncLifetime, IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly Guid _id = Guid.NewGuid();

    public LifecycleTests(ITestOutputHelper output)
    {
        _output = output;
        
        _output.WriteLine($"Hello from sync setup: {_id}");
    }

    [Fact]
    public void Test()
    {
        _output.WriteLine($"Hello from test: {_id}");
    }
    
    [Fact]
    public void Test2()
    {
        _output.WriteLine($"Hello from test: {_id}");
    }

    public void Dispose()
    {
        _output.WriteLine($"Hello from sync cleanup: {_id}");
    }

    public Task InitializeAsync()
    {
        _output.WriteLine($"Hello from async setup: {_id}");
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _output.WriteLine($"Hello from async cleanup: {_id}");
        return Task.CompletedTask;
    }
}
