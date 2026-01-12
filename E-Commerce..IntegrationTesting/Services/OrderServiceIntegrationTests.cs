
using Causmatic_backEnd.Application.Interfaces.Authentication;
using Causmatic_backEnd.Application.Interfaces.Core;
using Causmatic_backEnd.Application.Services.Core;
using Causmatic_backEnd.Core.Data;
using Causmatic_backEnd.Repository.Repositories.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Causmatic_backEnd.IntegrationTesting.Services;
public class OrderServiceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly StoreContext _context;
    private readonly OrderService _orderService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSenderService _emailSenderService;
    public OrderServiceIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var scope = factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<StoreContext>();
        _userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        _emailSenderService = scope.ServiceProvider.GetRequiredService<IEmailSenderService>();
        _orderService = new OrderService(    _unitOfWork,_userManager,_context,_emailSenderService); 

        SeedTestData();
    }

    private void SeedTestData()
    {
        // Clear existing data
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        // Add test user
        var user = new AppUser { Email = "test@example.com", UserName = "test@example.com" };
        _userManager.CreateAsync(user, "Password123!").Wait();

        // Add test products
        _context.Products.AddRange(
            new Product { Name = "Product1", Price = 10.0m, StockQuantity = 5 },
            new Product { Name = "Product2", Price = 20.0m, StockQuantity = 3 }
        );
        _context.SaveChanges();
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task CreateOrder_EmptyItems_ReturnsBadRequest()
    {
        // Arrange
        var order = new Order { BuyerEmail = "test@example.com", Items = new List<OrderItem>() };

        // Act
        var result = await _orderService.CreateOrder(order);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task CreateOrder_InvalidUser_ReturnsBadRequest()
    {
        // Arrange
        var order = new Order
        {
            BuyerEmail = "invalid@example.com",
            Items = new List<OrderItem>
            {
                new OrderItem { Product = new Product { Name = "Product1" }, Quantity = 1 }
            }
        };

        // Act
        var result = await _orderService.CreateOrder(order);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Title.Should().Be("User Not Found");
    }

    [Fact]
    public async Task CreateOrder_WithNonExistingProduct_RemovesInvalidItem()
    {
        // Arrange
        var order = new Order
        {
            BuyerEmail = "test@example.com",
            Items = new List<OrderItem>
            {
                new OrderItem { Product = new Product { Name = "NonExisting" }, Quantity = 1 }
            }
        };

        // Act
        var result = await _orderService.CreateOrder(order);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        order.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateOrder_AdjustsQuantityToAvailableStock()
    {
        // Arrange
        var order = new Order
        {
            BuyerEmail = "test@example.com",
            Items = new List<OrderItem>
            {
                new OrderItem { Product = new Product { Name = "Product1" }, Quantity = 10 }
            }
        };

        // Act
        var result = await _orderService.CreateOrder(order);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Items.First().Quantity.Should().Be(5);
        _context.Products.Find("Product1").StockQuantity.Should().Be(0);
    }

    [Fact]
    public async Task CreateOrder_ValidOrder_CreatesSuccessfully()
    {
        // Arrange
        var order = new Order
        {
            BuyerEmail = "test@example.com",
            Items = new List<OrderItem>
            {
                new OrderItem { Product = new Product { Name = "Product1" }, Quantity = 2 },
                new OrderItem { Product = new Product { Name = "Product2" }, Quantity = 1 }
            }
        };

        // Act
        var result = await _orderService.CreateOrder(order);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Price.Should().Be(2 * 10m + 1 * 20m);
        order.OrderDate.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));

        // Verify database state
        var dbOrder = _context.Orders.Include(o => o.Items).First();
        dbOrder.Items.Should().HaveCount(2);
        _context.Products.Find("Product1").StockQuantity.Should().Be(3);
        _context.Products.Find("Product2").StockQuantity.Should().Be(2);
    }

    [Fact]
    public async Task CreateOrder_ZeroQuantityAfterAdjustment_RemovesItem()
    {
        // Arrange
        var product = _context.Products.Find("Product1");
        product.StockQuantity = 0;
        _context.SaveChanges();

        var order = new Order
        {
            BuyerEmail = "test@example.com",
            Items = new List<OrderItem>
            {
                new OrderItem { Product = new Product { Name = "Product1" }, Quantity = 1 }
            }
        };

        // Act
        var result = await _orderService.CreateOrder(order);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        order.Items.Should().BeEmpty();
    }
}