

using Causmatic_backEnd.Core.Data;

namespace Causmatic_backEnd.IntegrationTesting.Controllers;
public class AuthControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly StoreContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    private const string _testUserEmail = "test@example.com";
    private const string _testUserPassword = "TestPassword123!";

    public AuthControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication("TestScheme")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        "TestScheme", _ => { });
            });
        });

        var scope = _factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<StoreContext>();
        _userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        _tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        InitializeDatabaseAsync().Wait();
        _client = _factory.CreateClient();
    }

    private async Task InitializeDatabaseAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        var testUser = new AppUser
        {
            UserName = "testuser",
            Email = _testUserEmail,
            EmailConfirmed = true
        };
        await _userManager.CreateAsync(testUser, _testUserPassword);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task Register_ValidRequest_ReturnsSuccess()
    {
        var request = new RegisterRequest
        {
            Email = "new@example.com",
            Password = "NewPassword123!",
            ConfirmPassword = "NewPassword123!",
            FullName = "New User"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        content.Should().NotBeNull();
        content.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ConfirmEmail_ValidCode_ConfirmsEmail()
    {
        var user = await _userManager.FindByEmailAsync(_testUserEmail);
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        var request = new ConfirmEmailRequest
        {
            Email = _testUserEmail,
            Code = code
        };

        var response = await _client.PostAsJsonAsync("/api/auth/confirmemail", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        var request = new LoginRequest
        (
            Email : _testUserEmail,
            Password : _testUserPassword
        );

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        content.Should().NotBeNull();
        //content.Token.Should().NotBeNullOrEmpty();
    }

/*    [Fact]
    public async Task Logout_InvalidatesToken()
    {
        var loginRequest = new LoginRequest
        (
            Email : _testUserEmail,
            Password: _testUserPassword
        );
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResponse.Token);

        var logoutResponse = await _client.PostAsync("/api/auth/logout", null);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var jti = GetJtiFromToken(authResponse.Token);
        var isBlacklisted = await _tokenService.IsTokenBlacklistedAsync(jti);
        isBlacklisted.Should().BeTrue();
    }*/

    [Fact]
    public async Task GetCurrentUser_Authenticated_ReturnsUserDetails()
    {
        var loginRequest = new LoginRequest
        (
            Email : _testUserEmail,
            Password :_testUserPassword
        );
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

       /* _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResponse.Token);
*/
        var response = await _client.GetAsync("/api/auth/current-user");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var userResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        userResponse.Email.Should().Be(_testUserEmail);
    }

    [Fact]
    public async Task GoogleLogin_ReturnsChallengeResult()
    {
        var response = await _client.GetAsync("/api/auth/google-login");
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
    }

    private string GetJtiFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        return jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
    }
}

 