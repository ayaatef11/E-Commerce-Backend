

using Causmatic_backEnd.Core.Data;
using Causmatic_backEnd.DTOS.Cart.Responses;

namespace Causmatic_backEnd.IntegrationTesting.Controllers;
public class CartControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly StoreContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly string _testUserEmail = "test@example.com";
    private Product _existingProduct;

    public CartControllerIntegrationTests(WebApplicationFactory<Program> factory)
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
        _userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        InitializeDatabaseAsync().Wait();
        _client = _factory.CreateClient();
    }

    private async Task InitializeDatabaseAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
        var user = new IdentityUser { Email = _testUserEmail, UserName = _testUserEmail };
        await _userManager.CreateAsync(user, "Password123!");
        _existingProduct = new Product
        {
            Name = "Test Product",
            Price = 29.99m,
            StockQuantity = 10
        };
        _context.Products.Add(_existingProduct);
        await _context.SaveChangesAsync();
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task GetCart_ReturnsEmptyCartForNewUser()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _testUserEmail);
        var response = await _client.GetAsync("/cart/get");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cart = await response.Content.ReadFromJsonAsync<CartResponse>();
        cart.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task AddItemToCart_AddsNewItemSuccessfully()
    {
        var request = new { ProductId = _existingProduct.Id, Quantity = 2 };
        _client.DefaultRequestHeaders.Add("X-Test-Email", _testUserEmail);
        var response = await _client.PostAsJsonAsync("/cart/add/item",request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cart = await response.Content.ReadFromJsonAsync<CartResponse>();
        cart.Items.Should().ContainSingle();
        cart.Items.First().Quantity.Should().Be(2);
    }

    [Fact]
    public async Task UpdateItemQuantity_UpdatesExistingItem()
    {
        var request = new { ProductId = _existingProduct.Id, Quantity = 2 };
        _client.DefaultRequestHeaders.Add("X-Test-Email", _testUserEmail);
        await _client.PostAsJsonAsync("/cart/add/item",request );
        var response = await _client.PutAsJsonAsync($"/cart/update/item/{_existingProduct.Id}",
            new { NewQuantity = 5 });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cart = await response.Content.ReadFromJsonAsync<CartResponse>();
        cart.Items.First().Quantity.Should().Be(5);
    }

    [Fact]
    public async Task RemoveItemFromCart_DeletesItemSuccessfully()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _testUserEmail);
        await _client.PostAsJsonAsync("/cart/add/item",
            new { ProductId = _existingProduct.Id, Quantity = 2 });
        var response = await _client.DeleteAsync($"/cart/delete/item/{_existingProduct.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.EnsureSuccessStatusCode();
        var cartResponse = await response.Content.ReadFromJsonAsync<CartResponse>();
        cartResponse.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task ClearCart_RemovesAllItems()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _testUserEmail);
        await _client.PostAsJsonAsync("/cart/add/item",
            new { ProductId = _existingProduct.Id, Quantity = 2 });
        var response = await _client.DeleteAsync("/cart");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var getResponse = await _client.GetAsync("/cart/get");
        var cart = await getResponse.Content.ReadFromJsonAsync<CartResponse>();
        cart.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCartTotal_CalculatesCorrectTotal()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _testUserEmail);
        await _client.PostAsJsonAsync("/cart/add/item",
            new { ProductId = _existingProduct.Id, Quantity = 3 });
        var response = await _client.GetAsync("/cart/total");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var total = await response.Content.ReadFromJsonAsync<decimal>();
        total.Should().Be(3 * 29.99m);
    }

    [Fact]
    public async Task AddItemToCart_InvalidProduct_ReturnsNotFound()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _testUserEmail);
        var response = await _client.PostAsJsonAsync("/cart/add/item",
            new { ProductId = 999, Quantity = 1 });
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateItemQuantity_NonExistentItem_ReturnsNotFound()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _testUserEmail);

        var response = await _client.PutAsJsonAsync($"/cart/update/item/999",
            new { NewQuantity = 5 });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveItemFromCart_NonExistentItem_ReturnsNotFound()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _testUserEmail);

        var response = await _client.DeleteAsync($"/cart/delete/item/999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UnauthenticatedAccess_ReturnsUnauthorized()
    {
        var endpoints = new[]
        {
            ("GET", "/cart/get"),
            ("POST", "/cart/add/item"),
            ("PUT", "/cart/update/item/1"),
            ("DELETE", "/cart/delete/item/1"),
            ("DELETE", "/cart"),
            ("GET", "/cart/total")
        };

        foreach (var (method, path) in endpoints)
        {
            var request = new HttpRequestMessage(new HttpMethod(method), path);
            var response = await _client.SendAsync(request);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
