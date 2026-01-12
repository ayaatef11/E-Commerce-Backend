
using Causmatic_backEnd.Core.Data;

namespace Causmatic_backEnd.IntegrationTesting.Controllers;
public class InvoiceControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly StoreContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly string _adminEmail = "admin@example.com";
    private readonly string _userEmail = "user@example.com";
    private Invoice _testInvoice;
    private Order _testOrder;

    public InvoiceControllerIntegrationTests(WebApplicationFactory<Program> factory)
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

        InitializeDatabaseAsync().Wait();
        _client = _factory.CreateClient();
    }

    private async Task InitializeDatabaseAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        // Create admin user
        var adminUser = new AppUser { Email = _adminEmail, UserName = "admin" };
        await _userManager.CreateAsync(adminUser, "Admin123!");
        await _userManager.AddToRoleAsync(adminUser, Roles.Admin);

        // Create regular user
        var regularUser = new AppUser { Email = _userEmail, UserName = "user" };
        await _userManager.CreateAsync(regularUser, "User123!");
        await _userManager.AddToRoleAsync(regularUser, Roles.User);

        // Create test order
        _testOrder = new Order
        {
            BuyerEmail = _userEmail,
            Status = OrderStatus.Paid,
            Items = new List<OrderItem>
            {
                new OrderItem { Product = new Product { Name = "Test Product", Price = 100.00m }, Quantity = 2 }
            }
        };
        _context.Orders.Add(_testOrder);

        // Create test invoice
        _testInvoice = new Invoice
        {
            OrderId = _testOrder.Id,
            UserEmail = _userEmail,
            CreatedAt = DateTime.UtcNow,
            InvoiceStatus = Core.Shared.Utilties.Enums.Status.Pending
        };
        _context.Invoices.Add(_testInvoice);

        await _context.SaveChangesAsync();
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task GetAllInvoices_AdminAccess_ReturnsInvoices()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _adminEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.Admin);

        var response = await _client.GetAsync("/api/invoice/getAll");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var invoices = await response.Content.ReadFromJsonAsync<List<InvoiceResponse>>();
        invoices.Should().ContainSingle();
    }

    [Fact]
    public async Task GetInvoiceById_UserAccessOwnInvoice_ReturnsInvoice()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _userEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.User);

        var response = await _client.GetAsync($"/api/invoice/{_testInvoice.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var invoice = await response.Content.ReadFromJsonAsync<InvoiceResponse>();
        invoice.Id.Should().Be(_testInvoice.Id);
    }

    [Fact]
    public async Task CreateInvoice_ValidOrder_CreatesInvoice()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _userEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.User);

        var response = await _client.PostAsync($"/api/invoice/create/{_testOrder.Id}", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var createdInvoice = await response.Content.ReadFromJsonAsync<InvoiceResponse>();
        //createdInvoice.OrderId.Should().Be(_testOrder.Id);
    }

    [Fact]
    public async Task DownloadInvoice_UserAccess_ReturnsFile()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _userEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.User);

        var response = await _client.GetAsync($"/api/invoice/download/{_testInvoice.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType.MediaType.Should().Be("application/pdf");
    }

    [Fact]
    public async Task AdminDownloadInvoice_AdminAccess_ReturnsFile()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _adminEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.Admin);

        var response = await _client.GetAsync($"/api/invoice/AdminDownload/{_testInvoice.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType.MediaType.Should().Be("application/pdf");
    }

    [Fact]
    public async Task SendInvoice_AdminAccess_SendsInvoice()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _adminEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.Admin);

        var response = await _client.PostAsync($"/api/invoice/send/{_testInvoice.Id}", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedInvoice = await _context.Invoices.FindAsync(_testInvoice.Id);
        //updatedInvoice.Status.Should().Be(Status.Sent);
    }

    [Fact]
    public async Task GetUserInvoices_UserAccess_ReturnsInvoices()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _userEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.User);

        var response = await _client.GetAsync("/api/invoice/userInvoices");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var invoices = await response.Content.ReadFromJsonAsync<List<InvoiceResponse>>();
        invoices.Should().ContainSingle(i => i.Id == _testInvoice.Id);
    }

    [Fact]
    public async Task DeleteInvoice_AdminAccess_DeletesInvoice()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _adminEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.Admin);

        var response = await _client.DeleteAsync($"/api/invoice/delete/{_testInvoice.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var deletedInvoice = await _context.Invoices.FindAsync(_testInvoice.Id);
        deletedInvoice.Should().BeNull();
    }

    [Fact]
    public async Task AdminGetUserInvoices_ValidRequest_ReturnsInvoices()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _adminEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.Admin);

        var user = await _userManager.FindByEmailAsync(_userEmail);
        var response = await _client.GetAsync($"/api/invoice/admin-get-user-invoices/{user.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var invoices = await response.Content.ReadFromJsonAsync<List<InvoiceResponse>>();
        invoices.Should().ContainSingle(i => i.Id == _testInvoice.Id);
    }
}

