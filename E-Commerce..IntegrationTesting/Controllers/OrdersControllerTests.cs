using Causmatic_backEnd.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;
using System.Text;
using Xunit.Abstractions;

namespace Causmatic_backEnd.IntegrationTesting.Controllers;
public class OrdersControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly StoreContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly ITestOutputHelper _output;
    private Order _existingOrder;
    private OrderItem _existingItem;
    private Product _existingProduct;
    private const string _testUserEmail = "user@example.com";
    private const string _testAdminEmail = "admin@example.com";
    private Order _activeOrder;
    private Order _testOrder;
    private readonly string _testUserId = "test-user-id";

    private const string _otherUserEmail = "other@example.com";
    public OrdersControllerIntegrationTests(WebApplicationFactory<Program> factory,
        ITestOutputHelper output)
    {
        
        var scope = _factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<StoreContext>();
        _userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        InitializeDatabaseAsync().Wait();
        _client = _factory.CreateClient();
        _output = output;
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Configure test database
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<StoreContext>));
                services.AddAuthentication("TestScheme")
                   .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                       "TestScheme", options => { });
                services.Remove(descriptor);

                services.AddDbContext<StoreContext>(options =>
                {
                    //options.UseInMemoryDatabase("TestDatabase");
                });
                 
            });
        });
    }

    private async Task InitializeDatabaseAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        var user = new AppUser { Email = "test@example.com", UserName = "test@example.com" };
        await _userManager.CreateAsync(user, "Password123!");

        _context.Products.AddRange(
            new Product { Name = "Product1", Price = 10.0m, StockQuantity = 5 },
            new Product { Name = "Product2", Price = 20.0m, StockQuantity = 3 }
        );
        var regularUser = new AppUser { Email = _testUserEmail, UserName = _testUserEmail };
        await _userManager.CreateAsync(regularUser, "User123!");
        await _userManager.AddToRoleAsync(regularUser, Roles.User);
        await _context.SaveChangesAsync();
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task CreateOrder_Unauthenticated_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/orders/CreateOrder", new CreateOrderRequest());
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateOrder_InvalidRole_Returns403()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Roles", "Guest");
        var orderDto = new CreateOrderRequest { BuyerEmail = "test@example.com" };

        var response = await _client.PostAsJsonAsync("/api/orders/CreateOrder", orderDto);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateOrder_ValidRequest_ReturnsCreatedOrder()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Roles", "User");
        var orderDto = new CreateOrderRequest
        {
            BuyerEmail = "test@example.com",
            Items = new List<OrderItemRequest>
            {
                new OrderItemRequest { ProductName = "Product1", Quantity = 2 },
                new OrderItemRequest { ProductName = "Product2", Quantity = 1 }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/orders/CreateOrder", orderDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseOrder = await response.Content.ReadFromJsonAsync<OrderResponse>();
        responseOrder.Should().NotBeNull();
        responseOrder.Items.Should().HaveCount(2);
        responseOrder.Price.Should().Be(40.0m);
    }

    [Fact]
    public async Task CreateOrder_EmptyItems_ReturnsBadRequest()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Roles", "User");
        var orderDto = new CreateOrderRequest
        {
            BuyerEmail = "test@example.com",
            Items = new List<OrderItemRequest>()
        };

        var response = await _client.PostAsJsonAsync("/api/orders/CreateOrder", orderDto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Title.Should().Be("Empty Order");
    }

    [Fact]
    public async Task CreateOrder_InvalidUser_ReturnsBadRequest()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Roles", "User");
        var orderDto = new CreateOrderRequest
        {
            BuyerEmail = "invalid@example.com",
            Items = new List<OrderItemRequest>
            {
                new OrderItemRequest { ProductName = "Product1", Quantity = 1 }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/orders/CreateOrder", orderDto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Title.Should().Be("User Not Found");
    }

    [Fact]
    public async Task CreateOrder_InvalidProduct_ReturnsNotFound()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Roles", "User");
        var orderDto = new CreateOrderRequest
        {
            BuyerEmail = "test@example.com",
            Items = new List<OrderItemRequest>
            {
                new OrderItemRequest { ProductName = "InvalidProduct", Quantity = 1 }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/orders/CreateOrder", orderDto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Title.Should().Be("No Valid Items");
    }

    [Fact]
    public async Task CreateOrder_AdjustsQuantity_ReturnsAdjustedOrder()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Roles", "User");
        var orderDto = new CreateOrderRequest
        {
            BuyerEmail = "test@example.com",
            Items = new List<OrderItemRequest>
            {
                new OrderItemRequest { ProductName = "Product1", Quantity = 10 }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/orders/CreateOrder", orderDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseOrder = await response.Content.ReadFromJsonAsync<OrderResponse>();
        responseOrder.Items.First().Quantity.Should().Be(5);

        var product = await _context.Products.FindAsync("Product1");
        product.StockQuantity.Should().Be(0);
    }


    [Fact]
    public async Task CreateOrderFromCart_Unauthenticated_Returns401()
    {
        var response = await _client.PostAsync("/api/cart/create-order-from-cart", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateOrderFromCart_WithoutProperRole_Returns403()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Roles", "Guest");
        //_client.DefaultRequestHeaders.Add("X-Test-UserId", _testUserId);

        var response = await _client.PostAsync("/api/cart/create-order-from-cart", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateOrderFromCart_ValidCart_ReturnsOrder()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Roles", "User");
        //_client.DefaultRequestHeaders.Add("X-Test-UserId", _testUserId);

        var response = await _client.PostAsync("/api/cart/create-order-from-cart", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var orderResponse = await response.Content.ReadFromJsonAsync<OrderResponse>();
        orderResponse.Should().NotBeNull();
        orderResponse.Items.Should().HaveCount(2);
        orderResponse.Price.Should().Be(2 * 10m + 1 * 20m);

        //var cart =  _context.Carts
        //    .Include(c => c.Items);
        //    //.FirstOrDefaultAsync(c => c.UserId == _testUserId);
        //cart.Items.Should().BeEmpty();
        var product1 = await _context.Products.FindAsync(1);
        product1.StockQuantity.Should().Be(3);
        var product2 = await _context.Products.FindAsync(2);
        product2.StockQuantity.Should().Be(2);
    }

    [Fact]
    public async Task CreateOrderFromCart_EmptyCart_ReturnsBadRequest()
    {
        // Clear cart items
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstAsync();
        cart.Items.Clear();
        await _context.SaveChangesAsync();

        _client.DefaultRequestHeaders.Add("X-Test-Roles", "User");
        //_client.DefaultRequestHeaders.Add("X-Test-UserId", _testUserId);

        var response = await _client.PostAsync("/api/cart/create-order-from-cart", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        //problem.Title.Should().Contain("empty", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateOrderFromCart_InsufficientStock_AdjustsQuantities()
    {
        // Update product stock
        var product = await _context.Products.FindAsync(1);
        product.StockQuantity = 1;
        await _context.SaveChangesAsync();

        _client.DefaultRequestHeaders.Add("X-Test-Roles", "User");
        //_client.DefaultRequestHeaders.Add("X-Test-UserId", _testUserId);

        var response = await _client.PostAsync("/api/cart/create-order-from-cart", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var orderResponse = await response.Content.ReadFromJsonAsync<OrderResponse>();
        var item = orderResponse.Items.First(i => i.Id == 1);
        item.Quantity.Should().Be(1);
        item.Price.Should().Be(1 * 10m);

        // Verify stock is 0
        var updatedProduct = await _context.Products.FindAsync(1);
        updatedProduct.StockQuantity.Should().Be(0);
    }




  /*  [Fact]
    public async Task GetOrderById_Unauthenticated_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync($"/api/orders/getOrder/{_testOrder.Id}?buyerEmail={_testUserEmail}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }*/

    [Fact]
    public async Task GetOrderById_ValidUser_ReturnsOrder()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _testUserEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.User);
        var response = await _client.GetAsync($"/api/orders/getOrder/{_testOrder.Id}?buyerEmail={_testUserEmail}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orderResponse = await response.Content.ReadFromJsonAsync<OrderResponse>();
        orderResponse.Should().NotBeNull();
        orderResponse.Id.Should().Be(_testOrder.Id);
        orderResponse.BuyerEmail.Should().Be(_testUserEmail);
    }

    [Fact]
    public async Task GetOrderById_NonExistentOrder_ReturnsNotFound()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Test-Email", _testUserEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.User);

        // Act
        var response = await _client.GetAsync($"/api/orders/getOrder/9999?buyerEmail={_testUserEmail}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain($"Order with ID 9999 not found");
    }

    [Fact]
    public async Task GetOrderById_AdminAccess_ReturnsAnyOrder()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _testAdminEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.Admin);
        var response = await _client.GetAsync($"/api/orders/getOrder/{_testOrder.Id}?buyerEmail={_testUserEmail}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orderResponse = await response.Content.ReadFromJsonAsync<OrderResponse>();
        orderResponse.Id.Should().Be(_testOrder.Id);
    }

    [Fact]
    public async Task GetOrderById_WrongUser_ReturnsForbidden()
    {
        var anotherUserEmail = "another@example.com";
        _client.DefaultRequestHeaders.Add("X-Test-Email", anotherUserEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.User);

        var response = await _client.GetAsync($"/api/orders/getOrder/{_testOrder.Id}?buyerEmail={anotherUserEmail}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
 

    [Fact]
    public async Task AddProductToOrder_Unauthenticated_ReturnsUnauthorized()
    {
        var request = new OrderItemRequest {   Quantity = 1 };
        var response = await _client.PostAsJsonAsync($"/orders/{_existingOrder.Id}/addItem", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddProductToOrder_ValidUser_AddsItemToOrder()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _testUserEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.User);

        var request = new OrderItemRequest
        {
            Quantity = 2
        };

        var response = await _client.PostAsJsonAsync($"/orders/{_existingOrder.Id}/addItem", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var orderResponse = await response.Content.ReadFromJsonAsync<OrderResponse>();
        orderResponse.Items.Should().HaveCount(1);
        orderResponse.Items.First().Id.Should().Be(_existingProduct.Id);

        var updatedOrder = await _context.Orders
            .Include(o => o.Items)
            .FirstAsync(o => o.Id == _existingOrder.Id);
        updatedOrder.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddProductToOrder_AdminUser_CanModifyAnyOrder()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _testAdminEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.Admin);

        var request = new OrderItemRequest
        {
            Quantity = 1
        };
        var response = await _client.PostAsJsonAsync($"/orders/{_existingOrder.Id}/addItem", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AddProductToOrder_InvalidProduct_ReturnsBadRequest()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _testUserEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.User);

        var request = new OrderItemRequest
        {
            Quantity = 1
        };

        var response = await _client.PostAsJsonAsync($"/orders/{_existingOrder.Id}/addItem", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Title.Should().Contain("Product");
    }

    [Fact]
    public async Task AddProductToOrder_InsufficientStock_AdjustsQuantity()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _testUserEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.User);

        var request = new OrderItemRequest
        {
            Quantity = 15  
        };

        var response = await _client.PostAsJsonAsync($"/orders/{_existingOrder.Id}/addItem", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orderResponse = await response.Content.ReadFromJsonAsync<OrderResponse>();
        orderResponse.Items.First().Quantity.Should().Be(10);
        var updatedProduct = await _context.Products.FindAsync(_existingProduct.Id);
        updatedProduct.StockQuantity.Should().Be(0);
    }

    [Fact]
    public async Task AddProductToOrder_UnauthorizedUser_ReturnsForbidden()
    {
        // Arrange
        var otherUserEmail = "other@example.com";
        _client.DefaultRequestHeaders.Add("X-Test-Email", otherUserEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.User);

        var request = new OrderItemRequest
        {
            Quantity = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/orders/{_existingOrder.Id}/addItem", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
 

    private async Task<Order> CreateTestOrderWithItemsAsync()
    {
        var order = new Order
        {
            BuyerEmail = "test-user@gmail.com",
            Items = new List<OrderItem>
            {
                new OrderItem { ProductId = 1, Quantity = 2 },
                new OrderItem { ProductId = 2, Quantity = 1 }
            }
        };

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
        return order;
    }

    private void AuthenticateClient(string role = Roles.User)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user"),
            new Claim(ClaimTypes.Role, role)
        };

        var authToken = new JwtSecurityToken(
            issuer: "test-issuer",
            audience: "test-audience",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test-secret-key")),
                SecurityAlgorithms.HmacSha256)
        );

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(authToken));
    }

    [Fact]
    public async Task RemoveProductFromOrder_ValidRequest_ReturnsUpdatedOrder()
    {
        // Arrange
        var order = await CreateTestOrderWithItemsAsync();
        var itemToRemove = order.Items.First();
        AuthenticateClient();

        // Act
        var response = await _client.DeleteAsync($"/api/orders/{order.Id}/deleteItem?itemId={itemToRemove.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<OrderResponse>();
        content.Should().NotBeNull();
        content.Items.Should().HaveCount(1);
        content.Items.Should().NotContain(i => i.Id == itemToRemove.Id);

        // Verify database state
        var dbOrder = await _context.Orders.FindAsync(order.Id);
        dbOrder.Items.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(null)]  
    [InlineData("Guest")]  
    public async Task RemoveProductFromOrder_Unauthorized_ReturnsForbidden(string? role)
    {
        var order = await CreateTestOrderWithItemsAsync();
        var itemToRemove = order.Items.First();

        if (role != null)
            AuthenticateClient(role);

        var response = await _client.DeleteAsync($"/api/orders/{order.Id}/deleteItem?itemId={itemToRemove.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RemoveProductFromOrder_InvalidOrderId_ReturnsNotFound()
    {
        AuthenticateClient();
        var invalidOrderId = 999;
        var validItemId = 1;
        var response = await _client.DeleteAsync($"/api/orders/{invalidOrderId}/deleteItem?itemId={validItemId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveProductFromOrder_InvalidItemId_ReturnsNotFound()
    {
        var order = await CreateTestOrderWithItemsAsync();
        AuthenticateClient();
        var invalidItemId = 999;

        var response = await _client.DeleteAsync($"/api/orders/{order.Id}/deleteItem?itemId={invalidItemId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveProductFromOrder_AdminRole_SuccessfulRemoval()
    {
        var order = await CreateTestOrderWithItemsAsync();
        var itemToRemove = order.Items.First();
        AuthenticateClient(Roles.Admin);
        var response = await _client.DeleteAsync($"/api/orders/{order.Id}/deleteItem?itemId={itemToRemove.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

 
    [Fact]
    public async Task UpdateProductInOrder_Unauthenticated_ReturnsUnauthorized()
    {
        var response = await _client.PutAsync(
            $"/orders/{_existingOrder.Id}/updateItem?itemId={_existingItem.Id}&newQuantity=3",
            null
        );
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateProductInOrder_ValidUser_UpdatesQuantity()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _testUserEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.User);

        const int newQuantity = 3;

        var response = await _client.PutAsync(
            $"/orders/{_existingOrder.Id}/updateItem?itemId={_existingItem.Id}&newQuantity={newQuantity}",
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var orderResponse = await response.Content.ReadFromJsonAsync<OrderResponse>();
        orderResponse.Items.First().Quantity.Should().Be(newQuantity);

        var updatedItem = await _context.OrderItems.FindAsync(_existingItem.Id);
        updatedItem.Quantity.Should().Be(newQuantity);
        _existingProduct.StockQuantity.Should().Be(10 - newQuantity);  
    }

    [Fact]
    public async Task UpdateProductInOrder_AdminUser_CanUpdateAnyOrder()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _testAdminEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.Admin);

        const int newQuantity = 4;
        var response = await _client.PutAsync(
            $"/orders/{_existingOrder.Id}/updateItem?itemId={_existingItem.Id}&newQuantity={newQuantity}",
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateProductInOrder_InvalidItemId_ReturnsNotFound()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Test-Email", _testUserEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.User);

        // Act
        var response = await _client.PutAsync(
            $"/orders/{_existingOrder.Id}/updateItem?itemId=999&newQuantity=3",
            null
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateProductInOrder_InsufficientStock_AdjustsQuantity()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _testUserEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.User);

        const int newQuantity = 15;  

        var response = await _client.PutAsync(
            $"/orders/{_existingOrder.Id}/updateItem?itemId={_existingItem.Id}&newQuantity={newQuantity}",
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orderResponse = await response.Content.ReadFromJsonAsync<OrderResponse>();
        orderResponse.Items.First().Quantity.Should().Be(10);
        _existingProduct.StockQuantity.Should().Be(0);
    }

    [Fact]
    public async Task UpdateProductInOrder_ZeroQuantity_RemovesItem()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _testUserEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.User);
        var response = await _client.PutAsync(
            $"/orders/{_existingOrder.Id}/updateItem?itemId={_existingItem.Id}&newQuantity=0",
            null
        );
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orderResponse = await response.Content.ReadFromJsonAsync<OrderResponse>();
        orderResponse.Items.Should().BeEmpty();
        _existingProduct.StockQuantity.Should().Be(10 + 2);  
    }

    [Fact]
    public async Task UpdateProductInOrder_UnauthorizedUser_ReturnsForbidden()
    {
        var otherUserEmail = "other@example.com";
        _client.DefaultRequestHeaders.Add("X-Test-Email", otherUserEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.User);
        var response = await _client.PutAsync(
            $"/orders/{_existingOrder.Id}/updateItem?itemId={_existingItem.Id}&newQuantity=3",
            null
        );
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateProductInOrder_NegativeQuantity_ReturnsBadRequest()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _testUserEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.User);
        var response = await _client.PutAsync(
            $"/orders/{_existingOrder.Id}/updateItem?itemId={_existingItem.Id}&newQuantity=-1",
            null
        );
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task CancelOrder_Unauthenticated_ReturnsUnauthorized()
    {
        var response = await _client.PostAsync(
            $"/orders/{_activeOrder.Id}/cancel?email={_testUserEmail}",
            null
        );
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CancelOrder_ValidUser_CancelsOwnOrder()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _testUserEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.User);

        var response = await _client.PostAsync(
            $"/orders/{_activeOrder.Id}/cancel?email={_testUserEmail}",
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var orderResponse = await response.Content.ReadFromJsonAsync<OrderResponse>();
        var updatedOrder = await _context.Orders.FindAsync(_activeOrder.Id);
        updatedOrder.Status.Should().Be(OrderStatus.Canceled);
    }

    [Fact]
    public async Task CancelOrder_Admin_CancelsOtherUserOrder()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _testAdminEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.Admin);

        var response = await _client.PostAsync(
            $"/orders/{_activeOrder.Id}/cancel?email={_testAdminEmail}",
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedOrder = await _context.Orders.FindAsync(_activeOrder.Id);
        updatedOrder.Status.Should().Be(OrderStatus.Canceled);
    }

    [Fact]
    public async Task CancelOrder_NonExistentOrder_ReturnsNotFound()
    {
        _client.DefaultRequestHeaders.Add("X-Test-Email", _testUserEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.User);

        var response = await _client.PostAsync(
            $"/orders/999/cancel?email={_testUserEmail}",
            null
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CancelOrder_UnauthorizedUser_ReturnsForbidden()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Test-Email", _otherUserEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.User);

        // Act
        var response = await _client.PostAsync(
            $"/orders/{_activeOrder.Id}/cancel?email={_otherUserEmail}",
            null
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CancelOrder_AlreadyCancelled_ReturnsConflict()
    {
        // Arrange
        _activeOrder.Status = OrderStatus.Canceled;
        await _context.SaveChangesAsync();

        _client.DefaultRequestHeaders.Add("X-Test-Email", _testUserEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.User);

        // Act
        var response = await _client.PostAsync(
            $"/orders/{_activeOrder.Id}/cancel?email={_testUserEmail}",
            null
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CancelOrder_RestoresProductStock()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Test-Email", _testUserEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", Roles.User);
        var initialStock = _context.Products.First().StockQuantity;

        // Act
        var response = await _client.PostAsync(
            $"/orders/{_activeOrder.Id}/cancel?email={_testUserEmail}",
            null
        );

        // Assert
        var updatedProduct = await _context.Products.FirstAsync();
        updatedProduct.StockQuantity.Should().Be(initialStock + 2);
    }

    [Fact]
    public async Task CancelOrder_InvalidRole_ReturnsForbidden()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Test-Email", _testUserEmail);
        _client.DefaultRequestHeaders.Add("X-Test-Roles", "Guest");

        // Act
        var response = await _client.PostAsync(
            $"/orders/{_activeOrder.Id}/cancel?email={_testUserEmail}",
            null
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}

