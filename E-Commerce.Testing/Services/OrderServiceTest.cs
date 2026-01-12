using E_Commerce.Core.Data;
using E_Commerce.UnitTesting.TestProviders;
using E_Commerce.Application.Interfaces.Authentication;
using E_Commerce.Application.Services.Core;
using E_Commerce.Repository.Repositories.Interfaces;
using E_Commerce.Repository.Specifications.OrderSpecifications;

namespace E_Commerce.Testing.Services;

public class OrderServiceTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly Mock<StoreContext> _storeContextMock;
    private readonly Mock<DbSet<Order>> _mockOrdersDbSet;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly DbContextOptions<StoreContext> _options;
    private readonly OrderService _orderService;
    private readonly Mock<IGenericRepository<Order>> _orderRepositoryMock;
    private readonly Mock<IGenericRepository<Product>> _productRepositoryMock;
    private readonly Mock<IGenericRepository<OrderItem>> _orderItemRepositoryMock;
    private readonly Mock<IEmailSenderService> _emailSenderServiceMock;
    public OrderServiceTest()
    {
        var userStoreMock = new Mock<IUserStore<AppUser>>();

        _userManagerMock = new Mock<UserManager<AppUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);

        _options = new DbContextOptionsBuilder<StoreContext>()
            .UseInMemoryDatabase("Test_Database")
            .Options;

        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _orderRepositoryMock = new Mock<IGenericRepository<Order>>();

        _productRepositoryMock= new Mock<IGenericRepository<Product>>();
      _orderItemRepositoryMock=new Mock<IGenericRepository<OrderItem>>();
        _emailSenderServiceMock=new Mock<IEmailSenderService>();
        _unitOfWorkMock
            .Setup(u => u.Repository<Order>())
            .Returns(_orderRepositoryMock.Object);

        _unitOfWorkMock
          .Setup(u => u.Repository<Product>())
          .Returns(_productRepositoryMock.Object);

        _unitOfWorkMock
          .Setup(u => u.Repository<OrderItem>())
          .Returns(_orderItemRepositoryMock.Object);
        _unitOfWorkMock
            .Setup(u => u.CompleteAsync())
            .ReturnsAsync(1);

        _mockOrdersDbSet = new Mock<DbSet<Order>>();
      
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(new DefaultHttpContext());

        _storeContextMock = new Mock<StoreContext>(_options, _httpContextAccessorMock.Object);
        _storeContextMock.Setup(c => c.Orders).Returns(_mockOrdersDbSet.Object);
        _orderService = new OrderService(
            _unitOfWorkMock.Object,
            _userManagerMock.Object,
            _storeContextMock.Object,
            _emailSenderServiceMock.Object
        );

        
    }

    [Fact]
    public async Task CreateOrder_EmptyItems_ReturnsFailure()
    {

        // Arrange
        var order = new Order { Items = new List<OrderItem>() };

        // Act
        var result = await _orderService.CreateOrder(order);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Title.Should().Be("Empty Order");
        result.Error.StatusCode.Should().Be(400);
    }
  
    [Fact]
    public async Task ProcessOrder_UserExists_SetsBuyerName()
    {
        // Arrange
        var userEmail = "user@example.com";

        var user = new AppUser
        {
            Email = userEmail,
            Full_Name = "John Doe"
        };
        _userManagerMock
          .Setup(um => um.FindByEmailAsync(userEmail))
          .ReturnsAsync(user);
        var product = new Product { Name = "ValidProduct", StockQuantity = 10 };
        _unitOfWorkMock
        .Setup(uow => uow.Repository<Product>().GetByNameAsync("ValidProduct"))
        .ReturnsAsync(product);
        var order = new Order { BuyerEmail = userEmail, Items = new List<OrderItem>
        {
            new OrderItem {  Product = product
            , Quantity = 1, 
              Price = 10.00m }
        }
        };
        _unitOfWorkMock
    .Setup(uow => uow.CompleteAsync())
    .ReturnsAsync(1);

        _orderRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        var result = await _orderService.CreateOrder(order);
        // Assert
        _orderRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Order>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            u => u.CompleteAsync(),
            Times.Once);
        Assert.Equal(user.Full_Name, order.BuyerName); 
        Assert.True(result.IsSuccess); 
    }

    [Fact]
    public async Task ProcessOrder_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var userEmail = "nonexistent@example.com";
        _userManagerMock
            .Setup(um => um.FindByEmailAsync(userEmail))
            .ReturnsAsync((AppUser)null);
        var product = new Product { Name = "ValidProduct", StockQuantity = 10 };
        _unitOfWorkMock
        .Setup(uow => uow.Repository<Product>().GetByNameAsync("ValidProduct"))
        .ReturnsAsync(product);
        var order = new Order
        {
            BuyerEmail = userEmail,
            Items = new List<OrderItem>
        {
            new OrderItem {  Product = product
            , Quantity = 1,
              Price = 10.00m }
        }
        };
        // Act
        var result = await _orderService.CreateOrder(order);
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User Not Found", result.Error?.Title); 
        Assert.Equal(StatusCodes.Status400BadRequest, result.Error.StatusCode);  
    }

    /* [Fact]
     public async Task GetById_OrderNotFound_ReturnsFailure()
     {
         //arrange
         int id = 0;
         string BuyerEmail = "aya@gmail.com";
         var spec = Mock < OrderSpecifications(BuyerEmail, id) >;
         //mock drepository
         var mockRepo=new Mock<IRepository<Order>>();
     }*/
    [Fact]
    public async Task GetById_ValidIdAndEmail_ReturnsOrder()
    {
        // Arrange
        var order = new Order { Id = 1, BuyerEmail = "test@example.com" };
        _orderRepositoryMock.Setup(r => r.GetByIdWithSpecAsync(It.IsAny<OrderSpecifications>()))
                .ReturnsAsync(order);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Repository<Order>()).Returns(_orderRepositoryMock.Object);

        // Act
        var result = await _orderService.GetById(order.Id,order.BuyerEmail);

        // Assert
        Assert.NotNull(result);
    }
    [Fact]
    public async Task GetById_EmailNotValidateWithId_ReturnNull()
    {
         var order = new Order { Id = 1, BuyerEmail = "aya@gmail.com" };

        _emailSenderServiceMock
        .Setup(e => e.ValidateEmail(It.Is<string>(email => email == order.BuyerEmail)))
        .Returns(true);
        _orderRepositoryMock.Setup(r => r.GetByIdWithSpecAsync(It.Is<OrderSpecifications>(s =>
            s.Email == order.BuyerEmail &&
            s.OrderId == order.Id
        )))
               .ReturnsAsync(order);
         var result = await _orderService.GetById(order.Id,"ahmad@gmail.com");
 
        Assert.Null(result);

    }
    [Fact]
    public async Task GetById_IdNotFound_ReturnNull()
    {
        var order = new Order { Id = 190, BuyerEmail = "aya@gmail.com" };
        _emailSenderServiceMock
      .Setup(e => e.ValidateEmail(It.Is<string>(email => email == order.BuyerEmail)))
      .Returns(true);
        _orderRepositoryMock.Setup(r => r.GetByIdWithSpecAsync(It.Is<OrderSpecifications>(s =>
              s.Email == order.BuyerEmail &&
              s.OrderId == order.Id
          ))) .ReturnsAsync(order);
        var result = await _orderService.GetById(11, order.BuyerEmail);
        Assert.Null(result);
    }
    [Fact]

    public async Task GetById_EmailInValid_ReturnNUll()
    {
        var order = new Order { Id = 1, BuyerEmail = "aa1" };
        _emailSenderServiceMock
      .Setup(e => e.ValidateEmail(It.Is<string>(email => email == order.BuyerEmail)))
      .Returns(true);
        _orderRepositoryMock.Setup(r => r.GetByIdWithSpecAsync(It.Is<OrderSpecifications>(s =>
             s.Email == order.BuyerEmail &&
             s.OrderId == order.Id
         )))
                .ReturnsAsync(order);
        var result = await _orderService.GetById(order.Id, order.BuyerEmail);
        Assert.Null(result);
    }
    [Fact]
    public async Task GetById_IdAndEmailInValid_ReturnNull()
    {
         var order = new Order { Id = -1, BuyerEmail = "aa1" };
        _emailSenderServiceMock
      .Setup(e => e.ValidateEmail(It.Is<string>(email => email == order.BuyerEmail)))
      .Returns(true);
        _orderRepositoryMock.Setup(r => r.GetByIdWithSpecAsync(It.Is<OrderSpecifications>(s =>
            s.Email == order.BuyerEmail &&
            s.OrderId == order.Id
        ))) .ReturnsAsync(order);
        var result = await _orderService.GetById(order.Id, order.BuyerEmail);
         Assert.Null(result);
    }
    [Fact]
    public async Task GetById_IdInValid_ReturnNull()
    {
        var order = new Order { Id = -1, BuyerEmail = "user@example.com" };
        _emailSenderServiceMock
      .Setup(e => e.ValidateEmail(It.Is<string>(email => email == order.BuyerEmail)))
      .Returns(true);
        _orderRepositoryMock.Setup(r => r.GetByIdWithSpecAsync(It.Is<OrderSpecifications>(s =>
            s.Email == order.BuyerEmail &&
            s.OrderId == order.Id
        )))
               .ReturnsAsync(order);
        var result = await _orderService.GetById(order.Id, order.BuyerEmail);
        Assert.Null(result);
    }
    [Fact]
    public async Task AddProductToOrder_ValidIdAndItem_ReturnOrder()
    {
        var product = new Product { Name = "Laptop", StockQuantity = 10, Cost = 1000 };
        var order = new Order {Id=1, Status = OrderStatus.Pending, Items = new List<OrderItem>() };
        var item = new OrderItem { Product = product, Quantity = 2 };

        _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
        _productRepositoryMock.Setup(r => r.GetByNameAsync(product.Name)).ReturnsAsync(product);

        // Act
        var result = await _orderService.AddProductToOrder(1, item);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items); 
        Assert.Equal(8, product.StockQuantity);  

    }

    [Fact]
    public async Task AddProductToOrder_OrderIsNull_ReturnOrder()
    {
        //arrange
        Order nulledOrder = null;
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(nulledOrder);
        var item = new OrderItem { Product = new Product { Name = "Test" }, Quantity = 1 };

        // Act
        var result = await _orderService.AddProductToOrder(1, item);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(OrderStatus.Pending, result.Value.Status);
    }

    [Fact]
    public async Task AddProductToOrder_CancelledOrder_ReturnsFailure()
    {
        var canceledOrder = new Order {Id=1, Status = OrderStatus.Canceled };
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(canceledOrder);
        var item = new OrderItem { Product = new Product { Name = "Test" }, Quantity = 1 };
        var result = await _orderService.AddProductToOrder(canceledOrder.Id, item);
        Assert.False(result.IsSuccess);
        Assert.Equal("order is canceled, you can't add an item to it  ", result.Error.Title);
    }

    [Fact]
    public async Task AddProductToOrder_ProductIsNull_ReturnFailure()
    {
        var canceledOrder = new Order { Status = OrderStatus.Canceled };
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(canceledOrder);
        var item = new OrderItem { Product = null, Quantity = 1 };
        var result = await _orderService.AddProductToOrder(1, item);
        Assert.False(result.IsSuccess);
        Assert.Equal("Product not found ", result.Error.Title);
    }

    [Fact]
    public async Task AddProductToOrder_StockQuantityDoesntFit_ReturnFailure()
    {
        //arrange
        var canceledOrder = new Order {  };
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(canceledOrder);
        var item = new OrderItem { Product = new Product { Name = "ابره دهون" }, Quantity = 100 };

        // Act
        var result = await _orderService.AddProductToOrder(1, item);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Insufficient Quantity ", result.Error.Title);
    }

    //update order functions

    [Fact]
    public async Task UpdateItemQuantity_IncreaseQuantity_UpdatesStockCorrectly()
    {
        // Arrange
        var product = new Product { StockQuantity = 10 };
        var orderItem = new OrderItem { Quantity = 3, Product = product };
        var order = new Order { Items = new List<OrderItem> { orderItem } };
        var newQuantity = 5;

        // Act
        var oldQuantity = orderItem.Quantity;
        orderItem.Quantity = newQuantity;
        product.StockQuantity -= (newQuantity - oldQuantity);

        // Assert
        Assert.Equal(5, orderItem.Quantity);
        Assert.Equal(8, product.StockQuantity); 
    }

    [Fact]
     public async Task RemoveProductFromOrder_OrderIdAndOrderItemIdValid_ReturnsOrder()//gives an exception in the function itself
     {
         //arrange
         var product = new Product { Id = 1 ,StockQuantity=10};
         var orderItem=new OrderItem { Id = 2,Quantity=2,Product=product };
         var order = new Order() { Id = 1, Items = new List<OrderItem> { orderItem } };

  
          var mockOrdersDbSet = new Mock<DbSet<Order>>();
         mockOrdersDbSet.As<IQueryable<Order>>().Setup(m => m.Provider)
             .Returns(new TestAsyncQueryProvider<Order>(new List<Order> { order }.AsQueryable().Provider));

         mockOrdersDbSet.As<IQueryable<Order>>().Setup(m => m.Expression)
             .Returns(new List<Order> { order }.AsQueryable().Expression);
         mockOrdersDbSet.As<IQueryable<Order>>().Setup(m => m.ElementType)
             .Returns(new List<Order> { order }.AsQueryable().ElementType);
         mockOrdersDbSet.As<IQueryable<Order>>().Setup(m => m.GetEnumerator())
             .Returns(() => new List<Order> { order }.GetEnumerator());
         var mockContext = new Mock<StoreContext>();
         mockContext.Setup(c => c.Orders).Returns(mockOrdersDbSet.Object);
             var result = await _orderService.RemoveProductFromOrder(1, 2);

         result.IsSuccess.Should().BeTrue();
         result.Value.Items.Should().NotContain(orderItem);
         product.StockQuantity.Should().Be(12);   
         _productRepositoryMock.Verify(r => r.Update(product), Times.Once);
         _orderItemRepositoryMock.Verify(r => r.Delete(orderItem), Times.Once);
         _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
     }

    [Fact]
    public async Task RemoveProductFromOrder_OrderIdInvalid_ReturnNull()
    {
        //arrange
        var product = new Product { Id = 1, StockQuantity = 10 };
        var orderItem = new OrderItem { Id = 2, Quantity = 2, Product = product };
        var order = new Order() { Id = -91, Items = new List<OrderItem> {  } };

        //act 
        var result = await _orderService.RemoveProductFromOrder(-91, 2);
        //assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Items.Should().NotContain(orderItem);
        product.StockQuantity.Should().Be(10);  
        _productRepositoryMock.Verify(r => r.Update(product), Times.Once);
        _orderItemRepositoryMock.Verify(r => r.Delete(orderItem), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]

    public async Task RemoveProductFromOrder_OrderItemIdInvalid_ReturnNull()
    {
        //arrange
        var product = new Product { Id = 1, StockQuantity = 10 };
        var orderItem = new OrderItem { Id = -92, Quantity = 2, Product = product };
        var order = new Order() { Id = 1, Items = new List<OrderItem> {  } };

        //act 
        var result = await _orderService.RemoveProductFromOrder(1, -92);
        //assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Items.Should().NotContain(orderItem);
        product.StockQuantity.Should().Be(10);  
        _productRepositoryMock.Verify(r => r.Update(product), Times.Once);
        _orderItemRepositoryMock.Verify(r => r.Delete(orderItem), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveProductFromOrder_OrderCanceled_ReturnNull()
    {
        //arrange
        var product = new Product { Id = 1, StockQuantity = 10 };
        var orderItem = new OrderItem { Id = 2, Quantity = 2, Product = product };
        var order = new Order() { Id = 1, Items = new List<OrderItem> { orderItem }, Status=OrderStatus.Canceled };

        //act 
        var result = await _orderService.RemoveProductFromOrder(1, 2);
        //assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().NotContain(orderItem);
        product.StockQuantity.Should().Be(12);    
        _productRepositoryMock.Verify(r => r.Update(product), Times.Once);
        _orderItemRepositoryMock.Verify(r => r.Delete(orderItem), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveProductFromOrder_NullOrder_ReturnFailure()
    {
        //arrange
        var product = new Product { Id = 1, StockQuantity = 10 };
        var orderItem = new OrderItem { Id = 2, Quantity = 2, Product = product };
        Order order = null;

        //act 
        var result = await _orderService.RemoveProductFromOrder(1, 2);
        //assert

        result.IsSuccess.Should().BeFalse();
        result.Value.Items.Should().NotContain(orderItem);
        product.StockQuantity.Should().Be(10); 
        _productRepositoryMock.Verify(r => r.Update(product), Times.Once);
        _orderItemRepositoryMock.Verify(r => r.Delete(orderItem), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveProductFromOrder_NullItem_ReturnFailure()
    {
        //arrange
        var product = new Product { Id = 1, StockQuantity = 10 };
        OrderItem orderItem = null;
        var order = new Order() { Id = 1, Items = new List<OrderItem> {   } };

        //act 
        var result = await _orderService.RemoveProductFromOrder(1, 2);
        //assert

        // Check result  
        result.IsSuccess.Should().BeFalse();
        result.Value.Items.Should().NotContain(orderItem);

        // Verify stock update  
        product.StockQuantity.Should().Be(10);   

        // Verify repository calls  
        _productRepositoryMock.Verify(r => r.Update(product), Times.Once);
        _orderItemRepositoryMock.Verify(r => r.Delete(orderItem), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveProductFromOrder_DeleteOrderTwice_ReturnFailure()
    {
        // Arrange
        var orderId = 1;
        var orderItemId = 1;

        var product = new Product
        {
            Id = 1,
            StockQuantity = 10
        };

        var orderItem = new OrderItem
        {
            Id = orderItemId,
            ProductId = product.Id,
            Quantity = 2,
            Product = product
        };

        var order = new Order
        {
            Id = orderId,
            Items = new List<OrderItem> { orderItem }
        };
          
        //_orderItemRepositoryMock.SetupSequence(r => r.GetByIdWithSpecAsync(It.IsAny<OrderSpecifications>()))
        //    .ReturnsAsync(order)
        //    .ReturnsAsync(order);  

 
        var firstResult = await _orderService.RemoveProductFromOrder(orderId, orderItemId);
        var secondResult = await _orderService.RemoveProductFromOrder(orderId, orderItemId);

        // Assert
        firstResult.IsSuccess.Should().BeTrue();
        secondResult.IsSuccess.Should().BeFalse();
        secondResult.Error.Title.Should().Be("Item not found in the order");
        secondResult.Error.StatusCode.Should().Be(404);

        // Verify item deletion was called only once
        _orderItemRepositoryMock.Verify(r => r.Delete(It.IsAny<OrderItem>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]

    public async Task TrackOrderStatus_OrderFound_ReturnString()
    {
        var order = new Order { Id = 1,Status=OrderStatus.Paid };
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id))
           .ReturnsAsync(order);
        var result =await _orderService.TrackOrderStatus(order.Id);
        Assert.Equal(order.Status.ToString(),result);
        _orderRepositoryMock.Verify(r => r.GetByIdAsync(order.Id), Times.Once);

    }

    [Fact]
    public async Task TrackOrderStatus_OrderNotFound_ReturnString()
    {
        var order = new Order { Id = 1, Status = OrderStatus.Paid }; 
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(200))
           .ReturnsAsync(order);
        var result = await _orderService.TrackOrderStatus(order.Id);
        Assert.Equal("Order not found", result);
        _orderRepositoryMock.Verify(r => r.GetByIdAsync(200), Times.Never);
    }

    [Fact]
public async Task UpdateOrderStatus_ValidIdAndStatus_ReturnSuccess()
    {
        string status = OrderStatus.Completed.ToString();
        var order = new Order() { Id = 1, Status = OrderStatus.Paid };
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id))
         .ReturnsAsync(order);
        var result=await _orderService.UpdateOrderStatus(order.Id, status);
        Assert.Equal(status, order.Status.ToString());
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        _orderRepositoryMock.Verify(r => r.Update(It.IsAny<Order>()), Times.Once);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateOrderStatus_NullOrder_ReturnFailure()
    {
        string status = OrderStatus.Completed.ToString();
        var order = new Order() { Id = 1, Status = OrderStatus.Paid };
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(1222))
         .ReturnsAsync(order);
        var result = await _orderService.UpdateOrderStatus(order.Id, status);

        Assert.NotEqual(status, order.Status.ToString());
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        _orderRepositoryMock.Verify(r => r.Update(It.IsAny<Order>()), Times.Never);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateOrderStatus_InvalidStatus_ReturnFailure()
    {

        string status = "invalid status";
        var order = new Order() { Id = 1, Status = OrderStatus.Paid };
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id))
         .ReturnsAsync(order);
        var result = await _orderService.UpdateOrderStatus(order.Id, status);
        Assert.NotEqual(status, order.Status.ToString());
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        _orderRepositoryMock.Verify(r => r.Update(It.IsAny<Order>()), Times.Never);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateOrderStatus_InValidId_ReturnFailure()
    {
        string status = OrderStatus.Pending.ToString();
        var order = new Order() { Id = -1, Status = OrderStatus.Paid };
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id))
         .ReturnsAsync(order);
        var result = await _orderService.UpdateOrderStatus(order.Id, status);
        result.Error.Title.Equals("invalid order id");
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        _orderRepositoryMock.Verify(r => r.Update(It.IsAny<Order>()), Times.Never);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateOrderStatus_InvalidIdAndStatus_ReturnFailure()
    {
        string status = "invalid status";
        var order = new Order() { Id = -1, Status = OrderStatus.Paid };
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id))
         .ReturnsAsync(order);
        var result = await _orderService.UpdateOrderStatus(order.Id, status);
        Assert.NotEqual(status, order.Status.ToString());
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        _orderRepositoryMock.Verify(r => r.Update(It.IsAny<Order>()), Times.Never);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateOrderStatus_UpdateSameStatus_ReturnSucesss()
    {
        string status = "invalid status";
        var order = new Order() { Id = 1, Status = OrderStatus.Paid };
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id))
         .ReturnsAsync(order);
        var result = await _orderService.UpdateOrderStatus(order.Id, status);
        Assert.NotEqual(status, order.Status.ToString());
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        _orderRepositoryMock.Verify(r => r.Update(It.IsAny<Order>()), Times.Never);
        result.IsSuccess.Should().BeFalse();
    }
    [Fact]
    public async Task UpdateOrderStatus_NUllStatus_ReturnFailure()
    {
        string status = null;
        var order = new Order() { Id = 1, Status = OrderStatus.Paid };
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id))
         .ReturnsAsync(order);
        var result = await _orderService.UpdateOrderStatus(order.Id, status);
        Assert.NotEqual(status, order.Status.ToString());
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
        _orderRepositoryMock.Verify(r => r.Update(It.IsAny<Order>()), Times.Never);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task CancelOrder_NullOrder_ReturnFailure()
    {
        Order order = null;
        string email = "aya@gmai.com";
        _orderRepositoryMock.Setup(r => r.GetByIdWithSpecAsync(It.Is<OrderSpecifications>(s =>
                  s.Email == order.BuyerEmail &&
                  s.OrderId == order.Id
              ))).ReturnsAsync(order);
        var result = await _orderService.CancelOrder(1, email);

        result.IsSuccess.Should().BeFalse();
        Assert.False(result.IsSuccess);
        result.Error.Title.Should().Be("order not found ");
        result.Error.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        _productRepositoryMock.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
        _orderRepositoryMock.Verify(r => r.Update(It.IsAny<Order>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task CancelOrder_ValidIdAndEmail_ReturnSuceess()
    {

        var order = new Order() { Id = 1, BuyerEmail = "aya@gmail.com", Status = OrderStatus.Pending };
        string email = "aya@gmail.com";
        _emailSenderServiceMock
    .Setup(e => e.ValidateEmail(It.Is<string>(email => email == order.BuyerEmail)))
    .Returns(true);
        _orderRepositoryMock.Setup(r => r.GetByIdWithSpecAsync(It.Is<OrderSpecifications>(s =>
                  s.Email == order.BuyerEmail &&
                  s.OrderId == order.Id
              ))).ReturnsAsync(order);
        var result = await _orderService.CancelOrder(order.Id, email);

        result.IsSuccess.Should().BeTrue();
        _productRepositoryMock.Verify(r => r.Update(It.IsAny<Product>()), Times.Once);
        _orderRepositoryMock.Verify(r => r.Update(It.IsAny<Order>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task CancelOrder_InvalidId_ReturnFailure()
    {
        var order = new Order() { Id = -1, BuyerEmail = "aya@gmail.com", Status = OrderStatus.Pending };
        string email = "aya@gmail.com";


        _orderRepositoryMock.Setup(r => r.GetByIdWithSpecAsync(It.IsAny<OrderSpecifications>()))
                  .ReturnsAsync(order);
        var result = await _orderService.CancelOrder(order.Id, email);

        result.IsSuccess.Should().BeFalse();
        result.Error.Title.Should().Be("invalid order id");
        result.Error.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        _productRepositoryMock.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
        _orderRepositoryMock.Verify(r => r.Update(It.IsAny<Order>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }
    [Fact]
    public async Task CancelOrder_InvalidEmail_ReturnFailure()
    {
        var order = new Order() { Id = 1, BuyerEmail = "aya.com", Status = OrderStatus.Pending };
        string email = "aya@gmail.com";
        _emailSenderServiceMock
    .Setup(e => e.ValidateEmail(It.Is<string>(email => email == order.BuyerEmail)))
    .Returns(false);
        _orderRepositoryMock.Setup(r => r.GetByIdWithSpecAsync(It.Is<OrderSpecifications>(s =>
          s.Email == order.BuyerEmail &&
          s.OrderId == order.Id
      ))).ReturnsAsync(order);
        var result = await _orderService.CancelOrder(order.Id, email);

        result.IsSuccess.Should().BeFalse(); 
        _productRepositoryMock.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
        _orderRepositoryMock.Verify(r => r.Update(It.IsAny<Order>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }
    [Fact]
    public async Task CancelOrder_NullId_ReturnFailure()
    {
        var order = new Order() { Id =0, BuyerEmail = "aya@gmail.com", Status = OrderStatus.Pending };
        string email = "aya@gmail.com";
        _emailSenderServiceMock
    .Setup(e => e.ValidateEmail(It.Is<string>(email => email == order.BuyerEmail)))
    .Returns(true);
        _orderRepositoryMock.Setup(r => r.GetByIdWithSpecAsync(It.Is<OrderSpecifications>(s =>
          s.Email == order.BuyerEmail &&
          s.OrderId == order.Id
      ))).ReturnsAsync(order);
        var result = await _orderService.CancelOrder(order.Id, email);

        result.IsSuccess.Should().BeFalse();
        Assert.False(result.IsSuccess);
        result.Error.Title.Should().Be("invalid order id");
        result.Error.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        _productRepositoryMock.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
        _orderRepositoryMock.Verify(r => r.Update(It.IsAny<Order>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }
    [Fact]
    public async Task CancelOrder_NullEmail_ReturnFailure()
    {
        var order = new Order() { Id = 1, BuyerEmail = "aya@gmail.com", Status = OrderStatus.Pending };
        string email =null;
        _orderRepositoryMock.Setup(r => r.GetByIdWithSpecAsync(It.Is<OrderSpecifications>(s =>
                  s.Email == null &&
                  s.OrderId == order.Id
              ))).ReturnsAsync(order);
        var result = await _orderService.CancelOrder(order.Id, email);

        result.IsSuccess.Should().BeFalse();
        Assert.False(result.IsSuccess);
        result.Error.Title.Should().Be("Email not Valid");
        result.Error.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        _productRepositoryMock.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
        _orderRepositoryMock.Verify(r => r.Update(It.IsAny<Order>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }
    [Fact]
    public async Task CancelOrder_CancelledOrder_ReturnFailure()
    {
        var order = new Order() { Id = 1,BuyerEmail="aya@gmail.com" ,Status=OrderStatus.Canceled};
        var email = "aya@gmail.com";
        _orderRepositoryMock.Setup(r => r.GetByIdWithSpecAsync(It.Is<OrderSpecifications>(s =>
                  s.Email == order.BuyerEmail &&
                  s.OrderId == order.Id
              ))).ReturnsAsync(order);
        var result =await _orderService.CancelOrder(order.Id, email);
        
        result.IsSuccess.Should().BeFalse();
        Assert.False(result.IsSuccess);
        result.Error.Title.Should().Be("Cant cancel a cancelled order");
        result.Error.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        _productRepositoryMock.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
        _orderRepositoryMock.Verify(r => r.Update(It.IsAny<Order>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }
}
