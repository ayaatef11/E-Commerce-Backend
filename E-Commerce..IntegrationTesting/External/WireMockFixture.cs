using WireMock.Server;
namespace Causmatic_backEnd.IntegrationTesting.External;
public class WireMockFixture : IAsyncLifetime
{
    public WireMockServer Server { get; private set; }

    public async Task InitializeAsync()
    {
        Server = WireMockServer.Start(); 
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        Server.Stop();  
        await Task.CompletedTask;
    }
}
