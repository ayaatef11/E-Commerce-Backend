using E_Commerce.Core.Data;
using E_Commerce.Core.Models.CartModels;
using E_Commerce.Application.Interfaces.Authentication;
using E_Commerce.Application.Services.Core;
using Microsoft.EntityFrameworkCore.Storage;

namespace E_Commerce.UnitTesting.Services;
    public class CartServiceTest
    {
    private readonly Mock<StoreContext>  _mockContext ;
    private readonly DbContextOptions<StoreContext> _options;
    private readonly CartService _cartService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly  Mock<DbSet<Cart>> _mockDbSet ;

    public CartServiceTest()
    {
        var userStoreMock = new Mock<IUserStore<AppUser>>();

        _mockContext = new Mock<StoreContext>();
        _mockUserService = new Mock<IUserService>();
        _userManagerMock = new Mock<UserManager<AppUser>>(
           userStoreMock.Object, null, null, null, null, null, null, null, null);
        _mockDbSet = new Mock<DbSet<Cart>>();
        _options = new DbContextOptionsBuilder<StoreContext>()
           .UseInMemoryDatabase("Test_Database")
           .Options;
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(new DefaultHttpContext());

        _mockContext = new Mock<StoreContext>(_options, _httpContextAccessorMock.Object);
        //_mockContext.Setup(c => c.Orders).Returns(_mockOrdersDbSet.Object);
        _cartService = new CartService(_mockContext.Object, _mockUserService.Object,_userManagerMock.Object);
        _mockUserService.Setup(u => u.GetCurrentUserId()).Returns("user123");


    }
    [Fact]
        public async Task GetUserCartAsync_UserHasCart_ReturnsCartWithItemsAndProducts()
        {
        var userId = "user123";
        _mockUserService.Setup(u => u.GetCurrentUserId()).Returns(userId);

        var mockCart = new Cart { UserId = userId, Items = new List<CartItem> { new CartItem { Product = new Product() } } };
        var mockCarts = new List<Cart> { mockCart }.AsQueryable();

        _mockDbSet.As<IQueryable<Cart>>().Setup(m => m.Expression).Returns(mockCarts.Expression);
        _mockDbSet.As<IQueryable<Cart>>().Setup(m => m.ElementType).Returns(mockCarts.ElementType);
        _mockDbSet.As<IQueryable<Cart>>().Setup(m => m.GetEnumerator()).Returns(mockCarts.GetEnumerator());

        _mockContext.Setup(c => c.Carts).Returns(_mockDbSet.Object);

        var result = await _cartService.GetUserCartAsync();

        Assert.NotNull(result); 
    }

    [Fact]
    public async Task AddItemToCartAsync_InvalidProduct_ReturnsFailure()
    {
        // Arrange
        _mockContext.Setup(c => c.Products.FindAsync(It.IsAny<int>())).ReturnsAsync((Product?)null);

        // Act
        var result = await _cartService.AddItemToCartAsync(999, 1);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("ProductNotFound", result.Error?.Title);
    }

    [Fact]
    public async Task AddItemToCartAsync_ValidItem_ReturnsSuccess()
    {
        // Arrange
        var product = new Product { Id = 1, StockQuantity = 10 };
        _mockContext.Setup(c => c.Products.FindAsync(1)).ReturnsAsync(product);
        //_mockContext.Setup(c => c.Carts).Returns(_mockDbSet(new List<Cart>()));

        var result = await _cartService.AddItemToCartAsync(1, 2);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Items.First().Quantity);
    }

    [Fact]
    public async Task AddItemToCartAsync_ValidIdAndQuantity_ReturnCart()
    {
        // Arrange
        var productId = 1;
        var quantity = 2;
        var userId = "user123";

        var product = new Product
        {
            Id = productId,
            StockQuantity = 10,
            Cost = 99.99m,
            Name = "Test Product",
            PictureUrl = "test.jpg"
        };

        var cart = new Cart
        {
            UserId = userId,
            Items = new List<CartItem>()
        };

        // Mock product lookup
        _mockContext.Setup(c => c.Products.FindAsync(productId))
                   .ReturnsAsync(product);

        var mockCarts = new List<Cart> { cart }.AsQueryable();
        _mockDbSet.As<IQueryable<Cart>>().Setup(m => m.Provider).Returns(mockCarts.Provider);
        _mockDbSet.As<IQueryable<Cart>>().Setup(m => m.GetEnumerator()).Returns(mockCarts.GetEnumerator());
        _mockContext.Setup(c => c.Carts).Returns(_mockDbSet.Object);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(1);

        // Act
        var result = await _cartService.AddItemToCartAsync(productId, quantity);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(userId, result.Value.UserId);

        // Verify item was added
        var item = result.Value.Items.FirstOrDefault();
        Assert.NotNull(item);
        Assert.Equal(productId, item.ProductId);
        Assert.Equal(quantity, item.Quantity);
        Assert.Equal(product.Cost, item.Price);

        _mockDbSet.Verify(d => d.Update(It.IsAny<Cart>()), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task AddItemToCartAsync_ProductNotFound_ReturnsFailure()
    {
        _mockContext.Setup(c => c.Products.FindAsync(It.IsAny<int>())).ReturnsAsync((Product?)null);

        var result = await _cartService.AddItemToCartAsync(999, 1);

        Assert.True(result.IsFailure);
        Assert.Equal("ProductNotFound", result.Error.Title);
        Assert.Equal(404, result.Error.StatusCode);
    }
    [Fact]
    public async Task AddItemToCartAsync_InsufficentQuantity_ReturnTheFoundQuantity()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            StockQuantity = 5 // Only 5 units available
        };
        var existingCart = new Cart
        {
            UserId = "user123",
            Items = new List<CartItem>() // Empty cart
        };

        // Mock product repository to return limited stock product
        _mockContext.Setup(c => c.Products.FindAsync(1)).ReturnsAsync(product);

        // Mock cart repository to return empty cart
        var mockCarts = new List<Cart> { existingCart }.AsQueryable();
        _mockDbSet.As<IQueryable<Cart>>().Setup(m => m.Provider).Returns(mockCarts.Provider);
        _mockContext.Setup(c => c.Carts).Returns(_mockDbSet.Object);

        // Act: Request 10 units (exceeds available stock)
        var result = await _cartService.AddItemToCartAsync(1, 10);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("InsufficientStock", result.Error.Title);
        Assert.Equal("Not enough stock available", result.Error.Message);
        Assert.Equal(400, result.Error.StatusCode);
    }
    [Fact]
    public async Task AddItemToCartAsync_NotFoundItem_ReturnCart()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            StockQuantity = 100,
            Cost = 50.0m,
            Name = "Test Product",
            PictureUrl = "test.jpg"
        };
        var existingCart = new Cart
        {
            UserId = "user123",
            Items = new List<CartItem>()  
        };

        _mockContext.Setup(c => c.Products.FindAsync(1)).ReturnsAsync(product);

        var mockCarts = new List<Cart> { existingCart }.AsQueryable();
        _mockDbSet.As<IQueryable<Cart>>().Setup(m => m.Provider).Returns(mockCarts.Provider);
        _mockContext.Setup(c => c.Carts).Returns(_mockDbSet.Object);

        var result = await _cartService.AddItemToCartAsync(1, 3);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items); // Verify 1 item added
        Assert.Equal(3, result.Value.Items.First().Quantity);
        Assert.Equal("Test Product", result.Value.Items.First().Name);
    }
    [Fact]
    public async Task AddItemToCartAsync_InvalidQuantity_ReturnsFailure()
    {
        // Arrange: Valid product but invalid quantity
        var product = new Product { Id = 1, StockQuantity = 10 };
        _mockContext.Setup(c => c.Products.FindAsync(1)).ReturnsAsync(product);

        // Act: Pass quantity = 0
        var result = await _cartService.AddItemToCartAsync(1, 0);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("invalid quantity", result.Error.Title);
        Assert.Equal(400, result.Error.StatusCode);
    }
    [Fact]
    public async Task AddItemToCartAsync_InvalidProductId_ReturnsFailure()
    {
        // Act: Pass productId = 0
        var result = await _cartService.AddItemToCartAsync(0, 1);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Product id is invalid  ", result.Error.Title);
        Assert.Equal(400, result.Error.StatusCode);
    }
    [Fact]
    public async Task AddItemToCartAsync_UnauthenticatedUser_ReturnsUnauthorized()
    {
        // Arrange: Simulate unauthenticated user
        _mockUserService.Setup(u => u.GetCurrentUserId()).Returns(string.Empty);

        // Act
        var result = await _cartService.AddItemToCartAsync(1, 1);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Unauthorized", result.Error.Title);
        Assert.Equal(401, result.Error.StatusCode);
    }
    [Fact]
    public async Task AddItemToCartAsync_InsufficientStock_ReturnsFailure()
    {
        // Arrange: Product with low stock
        var product = new Product { Id = 1, StockQuantity = 5 };
        _mockContext.Setup(c => c.Products.FindAsync(1)).ReturnsAsync(product);

        // Act: Request more than available stock
        var result = await _cartService.AddItemToCartAsync(1, 10);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("InsufficientStock", result.Error.Title);
        Assert.Equal(400, result.Error.StatusCode);
    }
    [Fact]
    public async Task AddItemToCartAsync_ValidItem_AddsNewItemToCart()
    {
        // Arrange: Empty cart and valid product
        var product = new Product { Id = 1, StockQuantity = 10, Cost = 100, Name = "Test Product" };
        var cart = new Cart { UserId = "user123", Items = new List<CartItem>() };

        // Mock setup
        _mockContext.Setup(c => c.Products.FindAsync(1)).ReturnsAsync(product);
        var mockCarts = new List<Cart> { cart }.AsQueryable();
        _mockDbSet.As<IQueryable<Cart>>().Setup(m => m.Provider).Returns(mockCarts.Provider);
        _mockContext.Setup(c => c.Carts).Returns(_mockDbSet.Object);

        // Act
        var result = await _cartService.AddItemToCartAsync(1, 2);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items); // Ensure item is added
        Assert.Equal(2, result.Value.Items.First().Quantity);
    }
    [Fact]
    public async Task AddItemToCartAsync_ExistingItem_UpdatesQuantity()
    {
        // Arrange: Cart with existing item
        var product = new Product { Id = 1, StockQuantity = 10 };
        var existingItem = new CartItem { ProductId = 1, Quantity = 2 };
        var cart = new Cart { UserId = "user123", Items = new List<CartItem> { existingItem } };

        // Mock setup
        _mockContext.Setup(c => c.Products.FindAsync(1)).ReturnsAsync(product);
        var mockCarts = new List<Cart> { cart }.AsQueryable();
        _mockDbSet.As<IQueryable<Cart>>().Setup(m => m.Provider).Returns(mockCarts.Provider);
        _mockContext.Setup(c => c.Carts).Returns(_mockDbSet.Object);

        // Act: Add more quantity
        var result = await _cartService.AddItemToCartAsync(1, 3);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.Items.First().Quantity); // 2 + 3 = 5
    }
    [Fact]
    public async Task AddItemToCartAsync_DatabaseError_ReturnsFailure()
    {
        // Arrange: Valid product but database error
        var product = new Product { Id = 1, StockQuantity = 10 };
        _mockContext.Setup(c => c.Products.FindAsync(1)).ReturnsAsync(product);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("DB error"));

        // Act
        var result = await _cartService.AddItemToCartAsync(1, 2);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("DatabaseError", result.Error.Title);
        Assert.Equal(500, result.Error.StatusCode);
    }
    [Fact]
    public async Task UpdateCartItemQuantityAsync_NotValidQuantity_ReturnNull()
    {
        // Arrange
        var productId = 1;
        var invalidQuantity = 0;
        var existingItem = new CartItem { ProductId = productId, Quantity = 2 };
        var cart = new Cart
        {
            UserId = "user123",
            Items = new List<CartItem> { existingItem }
        };

        // Mock dependencies
        _mockContext.Setup(c => c.Products.FindAsync(productId))
                   .ReturnsAsync(new Product { Id = productId, StockQuantity = 10 });
        SetupCartWithItems(cart);

        // Act
        var result = await _cartService.UpdateCartItemQuantityAsync(productId, invalidQuantity);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Quantity is invalid  ", result.Error.Title);
        Assert.Equal(400, result.Error.StatusCode);
    }

    [Fact]
    public async Task UpdateCartItemQuantityAsync_NotValidId_ReturnNull()
    {
        // Arrange
        var invalidProductId = 0;
        var validQuantity = 2;

        // Act
        var result = await _cartService.UpdateCartItemQuantityAsync(invalidProductId, validQuantity);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Product id is invalid  ", result.Error.Title);
        Assert.Equal(400, result.Error.StatusCode);
    }

    [Fact]
    public async Task UpdateCartItemQuantityAsync_ItemNotFound_ReturnNull()
    {
        // Arrange
        var productId = 1;
        var validQuantity = 2;
        var cart = new Cart
        {
            UserId = "user123",
            Items = new List<CartItem>() // Empty items
        };

        SetupCartWithItems(cart);
        _mockContext.Setup(c => c.Products.FindAsync(productId))
                   .ReturnsAsync(new Product { Id = productId });

        // Act
        var result = await _cartService.UpdateCartItemQuantityAsync(productId, validQuantity);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("item not found", result.Error.Title);
        Assert.Equal(404, result.Error.StatusCode);
    }

    [Fact]
    public async Task UpdateCartItemQuantityAsync_ProductNotFound_ReturnNull()
    {
        // Arrange
        var productId = 1;
        var validQuantity = 2;
        var existingItem = new CartItem { ProductId = productId };
        var cart = new Cart
        {
            UserId = "user123",
            Items = new List<CartItem> { existingItem }
        };

        SetupCartWithItems(cart);
        _mockContext.Setup(c => c.Products.FindAsync(productId))
                   .ReturnsAsync((Product?)null); // Product not found

        // Act
        var result = await _cartService.UpdateCartItemQuantityAsync(productId, validQuantity);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Product not found  ", result.Error.Title);
        Assert.Equal(404, result.Error.StatusCode);
    }




    [Fact]
    public async Task RemoveItemFromCartAsync_NotValidProductId_ReturnNull()
    {
        // Arrange
        var invalidProductId = 0;

        // Act
        var result = await _cartService.RemoveItemFromCartAsync(invalidProductId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Product id is invalid  ", result.Error.Title);
        Assert.Equal(StatusCodes.Status400BadRequest, result.Error.StatusCode);
    }

    [Fact]
    public async Task RemoveItemFromCartAsync_ItemNotFound_ReturnNull()
    {
        // Arrange
        var productId = 999;
        var existingCart = new Cart
        {
            UserId = "user123",
            Items = new List<CartItem>
        {
            new CartItem { ProductId = 1 },
            new CartItem { ProductId = 2 }
        }
        };

        // Mock cart retrieval
        SetupCartWithItems(existingCart);

        // Act
        var result = await _cartService.RemoveItemFromCartAsync(productId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Item not found", result.Error.Title);
        Assert.Equal(StatusCodes.Status404NotFound, result.Error.StatusCode);
        Assert.Contains($"Product {productId} not found in cart", result.Error.Message);
    }

    [Fact]
    public async Task RemoveItemFromCartAsync_CarttNotFound_ReturnNull()
    {
        // Arrange
        var productId = 1;

        // Mock empty cart
        SetupCartWithItems(null); // Or empty cart list

        // Act
        var result = await _cartService.RemoveItemFromCartAsync(productId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Cart not found", result.Error.Title);
        Assert.Equal(StatusCodes.Status404NotFound, result.Error.StatusCode);
        Assert.Equal("User cart does not exist", result.Error.Message);
    }

 
    // Helper method to setup cart retrieval
    private void SetupCartWithItems(Cart cart)
    {
        var mockCarts = new List<Cart> { cart }.AsQueryable();
        _mockDbSet.As<IQueryable<Cart>>().Setup(m => m.Provider).Returns(mockCarts.Provider);
        _mockDbSet.As<IQueryable<Cart>>().Setup(m => m.GetEnumerator()).Returns(mockCarts.GetEnumerator());
        _mockContext.Setup(c => c.Carts).Returns(_mockDbSet.Object);
    }
    [Fact]
    public async Task RemoveItemFromCartAsync_ValidRequest_UpdatesCart()
    {
        // Arrange
        var productId = 2;
        var existingCart = new Cart
        {
            UserId = "user123",
            Items = new List<CartItem>
        {
            new CartItem { ProductId = 1 },
            new CartItem { ProductId = 2 }
        }
        };

        //SetupCartWithItems(existingCart);
        //_mockContext.Setup(c => c.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _cartService.RemoveItemFromCartAsync(productId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.DoesNotContain(result.Value.Items, i => i.ProductId == productId);
    }
    [Fact]
    public async Task RemoveItemFromCartAsync_DatabaseError_ReturnsFailure()
    {
        // Arrange
        var productId = 1;
        var existingCart = new Cart
        {
            UserId = "user123",
            Items = new List<CartItem> { new CartItem { ProductId = 1 } }
        };

        SetupCartWithItems(existingCart);
        //_mockContext.Setup(c => c.SaveChangesAsync())
        //           .ThrowsAsync(new Exception("Database failure"));

        var result = await _cartService.RemoveItemFromCartAsync(productId);

        Assert.True(result.IsFailure);
        Assert.Equal("Database Error", result.Error.Title);
        Assert.Equal(500, result.Error.StatusCode);
    }
    [Fact]
    public async Task ClearCartAsync_FoundCart_ReturnSuccess()
    {
        // Arrange
        var userId = "user123";
        var existingCart = new Cart
        {
            UserId = userId,
            Items = new List<CartItem>
        {
            new CartItem { ProductId = 1 },
            new CartItem { ProductId = 2 }
        }
        };

        _mockUserService.Setup(u => u.GetCurrentUserId()).Returns(userId);
        SetupCartWithItems(existingCart);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(1);

        // Act
        var result = await _cartService.ClearCartAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(existingCart.Items); 

        _mockContext.Verify(c => c.Carts.Update(existingCart), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ClearCartAsync_NotFoundCart_ReturnFailure()
    {
        // Arrange
        var userId = "user123";

        // Mock cart retrieval to return null cart
        _mockUserService.Setup(u => u.GetCurrentUserId()).Returns(userId);
        SetupCartWithItems(null); // No cart exists

        // Act
        var result = await _cartService.ClearCartAsync();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Cart not found", result.Error.Title);
        Assert.Equal(StatusCodes.Status404NotFound, result.Error.StatusCode);
        Assert.Equal("User cart does not exist", result.Error.Message);

        // Verify no database operations occurred
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact]
    public async Task ClearCartAsync_DatabaseError_ReturnsFailure()
    {
        // Arrange
        var existingCart = new Cart { UserId = "user123", Items = new List<CartItem>() };
        SetupCartWithItems(existingCart);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new Exception("DB error"));

        // Act
        var result = await _cartService.ClearCartAsync();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Database Error", result.Error.Title);
        Assert.Equal(500, result.Error.StatusCode);
    }
    [Fact]
    public async Task ClearCartAsync_EmptyCart_ReturnsSuccess()
    {
        // Arrange
        var existingCart = new Cart { UserId = "user123", Items = new List<CartItem>() };
        SetupCartWithItems(existingCart);

        // Act
        var result = await _cartService.ClearCartAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(existingCart.Items);
    }
    [Fact]
    public async Task CalculateCartTotalAsync_DecimalPrecision_HandlesCorrectly()
    {
        // Arrange
        var mockCart = new Cart
        {
            Items = new List<CartItem>
        {
            new CartItem { Quantity = 3, Product = new Product { Cost = 9.99m } }
        }
        };
        SetupCartWithItems(mockCart);

        // Act
        var result = await _cartService.CalculateCartTotalAsync();

        // Assert
        Assert.Equal(29.97m, result.Value); 
    } 

        [Fact]
        public async Task CreateOrderFromCartAsync_UserNotAuthenticated_ReturnFailure()
        {
            // Arrange
            _mockUserService.Setup(u => u.GetCurrentUserId()).Returns((string)null);

            // Act
            var result = await _cartService.CreateOrderFromCartAsync();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(400);
        }

        [Fact]
        public async Task CreateOrderFromCartAsync_EmptyCart_ReturnFailure()
        {
            // Arrange
            var userId = "user123";
            var user = new AppUser { Id = userId };
            var cart = new Cart { UserId = userId, Items = new List<CartItem>() };

            _mockUserService.Setup(u => u.GetCurrentUserId()).Returns(userId);
            _userManagerMock.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);

            //var mockCarts = new List<Cart> { cart }.AsQueryable().BuildMock();
            //_mockContext.Setup(c => c.Carts).Returns(mockCarts.Object);

            // Act
            var result = await _cartService.CreateOrderFromCartAsync();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(404);
        }

        [Fact]
        public async Task CreateOrderFromCartAsync_ProductValidationFailure_ReturnFailure()
        {
            // Arrange
            var userId = "user123";
            var user = new AppUser { Id = userId };
            var productId = 1;
            var cartItem = new CartItem { ProductId = productId, Quantity = 2 };
            var cart = new Cart { UserId = userId, Items = new List<CartItem> { cartItem } };

            _mockUserService.Setup(u => u.GetCurrentUserId()).Returns(userId);
            _userManagerMock.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);

            //var mockCarts = new List<Cart> { cart }.AsQueryable().BuildMock();
            //_mockContext.Setup(c => c.Carts).Returns(mockCarts.Object);

            var mockProductsDbSet = new Mock<DbSet<Product>>();
            mockProductsDbSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync((Product)null);
            _mockContext.Setup(c => c.Products).Returns(mockProductsDbSet.Object);

            // Act
            var result = await _cartService.CreateOrderFromCartAsync();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(404);
        }

        [Fact]
        public async Task CreateOrderFromCartAsync_OrderCreation_ReturnSuccess()
        {
            // Arrange
            var userId = "user123";
            var user = new AppUser { Id = userId, Email = "test@example.com", Full_Name = "Test User" };
            var productId = 1;
            var product = new Product { Id = productId, StockQuantity = 10, Cost = 20.0m };
            var cartItem = new CartItem { ProductId = productId, Quantity = 2 };
            var cart = new Cart { UserId = userId, Items = new List<CartItem> { cartItem } };

            _mockUserService.Setup(u => u.GetCurrentUserId()).Returns(userId);
            _userManagerMock.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);

            //var mockCarts = new List<Cart> { cart }.AsQueryable().BuildMock();
            //_mockContext.Setup(c => c.Carts).Returns(mockCarts.Object);

            var mockProductsDbSet = new Mock<DbSet<Product>>();
            mockProductsDbSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync(product);
            _mockContext.Setup(c => c.Products).Returns(mockProductsDbSet.Object);

            var mockOrdersDbSet = new Mock<DbSet<Order>>();
            _mockContext.Setup(c => c.Orders).Returns(mockOrdersDbSet.Object);

            var mockTransaction = new Mock<IDbContextTransaction>();
            _mockContext.Setup(c => c.Database.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockTransaction.Object);

            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _cartService.CreateOrderFromCartAsync();

            // Assert
            result.IsSuccess.Should().BeTrue();
            mockOrdersDbSet.Verify(m => m.Add(It.IsAny<Order>()), Times.Once);
            mockProductsDbSet.Verify(m => m.Update(It.IsAny<Product>()), Times.Once);
            mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            cart.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateOrderFromCartAsync_DatabaseFailure_RollBack()
        {
            // Arrange
            var userId = "user123";
            var user = new AppUser { Id = userId };
            var productId = 1;
            var product = new Product { Id = productId, StockQuantity = 10 };
            var cartItem = new CartItem { ProductId = productId, Quantity = 2 };
            var cart = new Cart { UserId = userId, Items = new List<CartItem> { cartItem } };

            _mockUserService.Setup(u => u.GetCurrentUserId()).Returns(userId);
            _userManagerMock.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);

            //var mockCarts = new List<Cart> { cart }.AsQueryable().BuildMock();
            //_mockContext.Setup(c => c.Carts).Returns(mockCarts.Object);

            var mockProductsDbSet = new Mock<DbSet<Product>>();
            mockProductsDbSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync(product);
            _mockContext.Setup(c => c.Products).Returns(mockProductsDbSet.Object);

            var mockTransaction = new Mock<IDbContextTransaction>();
            _mockContext.Setup(c => c.Database.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockTransaction.Object);

            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _cartService.CreateOrderFromCartAsync();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(500);
            mockTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrderFromCartAsync_UserNotFound_ReturnFailure()
        {
            // Arrange
            var userId = "user123";
            _mockUserService.Setup(u => u.GetCurrentUserId()).Returns(userId);
            _userManagerMock.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync((AppUser)null);

            // Act
            var result = await _cartService.CreateOrderFromCartAsync();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(404);
        }

        [Fact]
        public async Task CreateOrderFromCartAsync_ConcurrencyIssues_ReturnFailure()
        {
            // Arrange
            var userId = "user123";
            var user = new AppUser { Id = userId };
            var productId = 1;
            var product = new Product { Id = productId, StockQuantity = 10 };
            var cartItem = new CartItem { ProductId = productId, Quantity = 2 };
            var cart = new Cart { UserId = userId, Items = new List<CartItem> { cartItem } };

            _mockUserService.Setup(u => u.GetCurrentUserId()).Returns(userId);
            _userManagerMock.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);

            //var mockCarts = new List<Cart> { cart }.AsQueryable().BuildMock();
            //_mockContext.Setup(c => c.Carts).Returns(mockCarts.Object);

            var mockProductsDbSet = new Mock<DbSet<Product>>();
            mockProductsDbSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync(product);
            _mockContext.Setup(c => c.Products).Returns(mockProductsDbSet.Object);

            var mockTransaction = new Mock<IDbContextTransaction>();
            _mockContext.Setup(c => c.Database.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockTransaction.Object);

            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateConcurrencyException("Concurrency error"));

            // Act
            var result = await _cartService.CreateOrderFromCartAsync();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(500);
            mockTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }





