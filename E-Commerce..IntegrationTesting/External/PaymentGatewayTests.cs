using WireMock.RequestBuilders;  
using WireMock.ResponseBuilders; 
namespace Causmatic_backEnd.IntegrationTesting.External;
public class MyApiTests : IClassFixture<WireMockFixture>
{
    private readonly WireMockFixture _fixture;

    public MyApiTests(WireMockFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TestExternalApiCall()
    {
        _fixture.Server
            .Given(Request.Create().WithPath("/api/data"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("{\"result\": \"success\"}"));

        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync($"{_fixture.Server.Urls[0]}/api/data");

        Assert.Equal(200, (int)response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("success", content);

        var requests = _fixture.Server.FindLogEntries(Request.Create().WithPath("/api/data"));
        Assert.Single(requests);
    }
}