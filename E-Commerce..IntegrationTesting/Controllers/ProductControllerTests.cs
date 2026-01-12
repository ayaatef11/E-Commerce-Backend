
using Causmatic_backEnd.Core.Data;
using Causmatic_backEnd.DTOS.Products.Requests;

namespace Causmatic_backEnd.IntegrationTesting.Controllers;
public class ProductControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly StoreContext _context;
    private readonly UserManager<AppUser> _userManager;
    private Product _testProduct;
    private const string _adminEmail = "admin@example.com";
    private const string _userEmail = "user@example.com";

    public ProductControllerIntegrationTests(WebApplicationFactory<Program> factory)
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

        // Create test users
        var adminUser = new AppUser { Email = _adminEmail, UserName = "admin" };
        await _userManager.CreateAsync(adminUser, "Admin123!");
        await _userManager.AddToRoleAsync(adminUser, Roles.Admin);

        var regularUser = new AppUser { Email = _userEmail, UserName = "user" };
        await _userManager.CreateAsync(regularUser, "User123!");
        await _userManager.AddToRoleAsync(regularUser, Roles.User);

        // Create test product
        _testProduct = new Product
        {
            Name = "Test Product",
            ListPrice = 100.00m,
            Mandop = 80.00m,
            GomlaPrice = 60.00m,
            StockQuantity = 10,
            IsDeleted = false
        };
        _context.Products.Add(_testProduct);
        await _context.SaveChangesAsync();
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task GetAllProducts_ReturnsPaginatedResults()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _userEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.User);

        var response = await _client.GetAsync("/api/product/GetAll?pageSize=5&pageIndex=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        //var products = await response.Content.ReadFromJsonAsync<ProductSpecification<Product>>();
        //products.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task FilterPrices_AdminAccess_UpdatesPrices()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _adminEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.Admin);

        var response = await _client.GetAsync("/api/product/filterPrices?filterType=gomla");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedProduct = await _context.Products.FindAsync(_testProduct.Id);
        updatedProduct.Price.Should().Be(_testProduct.GomlaPrice);
    }

    [Fact]
    public async Task CreateProduct_AdminAccess_CreatesProduct()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _adminEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.Admin);

        var newProduct = new CreateProductRequest
        {
            Name = "New Product"
        };

        var response = await _client.PostAsJsonAsync("/api/product/Create", newProduct);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetProductById_ReturnsProduct()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _userEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.User);

        var response = await _client.GetAsync($"/api/product/Get/{_testProduct.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var product = await response.Content.ReadFromJsonAsync<Product>();
        product.Name.Should().Be(_testProduct.Name);
    }

    //[Fact]
    //public async Task UpdateProduct_AdminAccess_UpdatesProduct()
    //{
    //    _client.DefaultRequestHeaders.Add("X-Test-Email", _adminEmail);
    //    _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.Admin);

    //    var updateRequest = new UpdateProductRequest
    //    {
    //        Name = "Updated Product" 
    //    };

    //    var response = await _client.PutAsJsonAsync($"/api/product/update/{_testProduct.Id}", updateRequest);
    //    response.StatusCode.Should().Be(HttpStatusCode.OK);
    //}

    [Fact]
    public async Task DeleteProduct_AdminAccess_DeletesProduct()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _adminEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.Admin);

        var response = await _client.DeleteAsync($"/api/product/Delete/{_testProduct.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var deletedProduct = await _context.Products.FindAsync(_testProduct.Id);
        deletedProduct.Should().BeNull();
    }

    [Fact]
    public async Task ArchiveProduct_AdminAccess_ArchivesProduct()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _adminEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.Admin);

        var response = await _client.PatchAsync($"/api/product/{_testProduct.Id}/archive", null);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var archivedProduct = await _context.Products.FindAsync(_testProduct.Id);
        archivedProduct.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task SearchProducts_ReturnsMatchingResults()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _userEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.User);

        var response = await _client.GetAsync("/api/product/Search?parameters=Test");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var products = await response.Content.ReadFromJsonAsync<List<Product>>();
        products.Should().ContainSingle(p => p.Name == "Test Product");
    }
}

 