
//using E_Commerce.Application.Interfaces.Authentication;
//using E_Commerce.Application.Services.Core;
//using E_Commerce.Core.Data;
//using E_Commerce.Core.Models.InvoiceModels;
//using E_Commerce.Core.Shared.Utilties.Enums;
//using E_Commerce.Repository.Specifications.InvoiceSpecifications;
//using Microsoft.AspNetCore.Mvc;

//namespace E_Commerce.Testing.Services;
 
//public class InvoiceServiceTest
//    {
//    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
//    private readonly Mock<UserManager<AppUser>> _userManagerMock;
//    private readonly Mock<StoreContext> _storeContextMock;
//    private readonly Mock<DbSet<Invoice>> _mockOrdersDbSet;
//    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
//    private readonly DbContextOptions<StoreContext> _options;
//    private readonly InvoiceService _invoiceService;
//    private readonly Mock<IGenericRepository<Product>> _productRepositoryMock;
//    private readonly Mock<IGenericRepository<OrderItem>> _orderItemRepositoryMock;
//    private readonly Mock<IEmailSenderService> _emailSenderServiceMock;
//    private readonly Mock<IGenericRepository<Order>> _orderRepositoryMock;
//    private readonly Mock<IGenericRepository<Invoice>> _invoiceRepositoryMock;

//    public InvoiceServiceTest()
//    {
//        var userStoreMock = new Mock<IUserStore<AppUser>>();

//        _userManagerMock = new Mock<UserManager<AppUser>>(
//            userStoreMock.Object, null, null, null, null, null, null, null, null);

//        _options = new DbContextOptionsBuilder<StoreContext>()
//            .UseInMemoryDatabase("Test_Database")
//            .Options;

//        _unitOfWorkMock = new Mock<IUnitOfWork>();
//        _productRepositoryMock = new Mock<IGenericRepository<Product>>();
//        _orderItemRepositoryMock = new Mock<IGenericRepository<OrderItem>>();
//        _emailSenderServiceMock = new Mock<IEmailSenderService>();

//        _unitOfWorkMock
//          .Setup(u => u.Repository<Product>())
//          .Returns(_productRepositoryMock.Object);

//        _unitOfWorkMock
//          .Setup(u => u.Repository<OrderItem>())
//          .Returns(_orderItemRepositoryMock.Object);
//        _unitOfWorkMock
//            .Setup(u => u.CompleteAsync())
//            .ReturnsAsync(1);

//        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
//        _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(new DefaultHttpContext());
//        _storeContextMock = new Mock<StoreContext>(_options, _httpContextAccessorMock.Object);
//        //_invoiceService = new InvoiceService(
//        //    _unitOfWorkMock.Object,
//        //    _userManagerMock.Object,
//        //    _httpContextAccessorMock.Object,
//        //    _emailSenderServiceMock.Object
//        //);

//        _orderRepositoryMock = new Mock<IGenericRepository<Order>>();
//        _invoiceRepositoryMock = new Mock<IGenericRepository<Invoice>>();

//        _unitOfWorkMock
//            .Setup(u => u.Repository<Order>())
//            .Returns(_orderRepositoryMock.Object);

//        _unitOfWorkMock
//            .Setup(u => u.Repository<Invoice>())
//            .Returns(_invoiceRepositoryMock.Object);
//    }


//        [Fact]
//        public async Task CreateInvoice_InvalidOrderId_ReturnFailure()
//        {
//            // Act
//            var result = await _invoiceService.Create_Invoice(0);

//            // Assert
//            result.IsSuccess.Should().BeFalse();
//            result.Error.Title.Should().Be("order id invalid");
//            result.Error.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
//        }

//        [Fact]
//        public async Task CreateInvoice_OrderNotFound_ReturnFailure()
//        {
//            // Arrange
//            var orderId = 1;
//            _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId))
//                .ReturnsAsync((Order)null);

//            // Act
//            var result = await _invoiceService.Create_Invoice(orderId);

//            // Assert
//            result.IsSuccess.Should().BeFalse();
//            result.Error.Title.Should().Be("Order not found");
//            result.Error.StatusCode.Should().Be(404);
//        }

//        [Fact]
//        public async Task CreateInvoice_UserNotFound_ReturnFailure()
//        {
//            // Arrange
//            var orderId = 1;
//            var order = new Order { Id = orderId, BuyerEmail = "test@example.com" };
//            _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId))
//                .ReturnsAsync(order);
//            _userManagerMock.Setup(m => m.FindByEmailAsync(order.BuyerEmail))
//                .ReturnsAsync((AppUser)null);

//            // Act
//            var result = await _invoiceService.Create_Invoice(orderId);

//            // Assert
//            result.IsSuccess.Should().BeFalse();
//            result.Error.Should().Be(404);
//        }

//        [Fact]
//        public async Task CreateInvoice_ValidRequest_ReturnsSuccessWithInvoice()
//        {
//            // Arrange
//            var orderId = 1;
//            var order = new Order
//            {
//                Id = orderId,
//                BuyerEmail = "test@example.com",
//                BuyerName = "Test User",
//                OrderDate = DateTime.UtcNow
//            };
//            var user = new AppUser { Email = "test@example.com" };
//            _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId))
//                .ReturnsAsync(order);
//            _userManagerMock.Setup(m => m.FindByEmailAsync(order.BuyerEmail))
//                .ReturnsAsync(user);
//            _invoiceRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Invoice>()))
//                .Returns(Task.CompletedTask);
//            _unitOfWorkMock.Setup(u => u.CompleteAsync())
//                .ReturnsAsync(1);

//            // Act
//            var result = await _invoiceService.Create_Invoice(orderId);

//            // Assert
//            result.IsSuccess.Should().BeTrue();
//            result.Value.InvoiceNumber.Should().StartWith($"INV-{orderId}-");
//            result.Value.TotalAmount.Should().Be(order.Price);
//            result.Value.UserEmail.Should().Be(order.BuyerEmail);
//            _invoiceRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Invoice>()), Times.Once);
//            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
//        }

//        [Fact]
//        public async Task CreateInvoice_DatabaseFailure_ReturnsDatabaseError()
//        {
//            // Arrange
//            var orderId = 1;
//            var order = new Order { Id = orderId, BuyerEmail = "test@example.com" };
//            var user = new AppUser { Email = "test@example.com" };

//            _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId))
//                .ReturnsAsync(order);
//            _userManagerMock.Setup(m => m.FindByEmailAsync(order.BuyerEmail))
//                .ReturnsAsync(user);
//            _invoiceRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Invoice>()))
//                .Returns(Task.CompletedTask);
//            _unitOfWorkMock.Setup(u => u.CompleteAsync())
//                .ThrowsAsync(new Exception("Database error"));

//            // Act
//            var result = await _invoiceService.Create_Invoice(orderId);

//            // Assert
//            result.IsSuccess.Should().BeFalse();
//            result.Error.Title.Should().Be("Database Error");
//            result.Error.Message.Should().Contain("Database error");
//            _invoiceRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Invoice>()), Times.Once);
//            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
//        }

//    [Fact]
//    public async Task DeleteInvoice_SuccessfulDeletion_DeleteAndReturnTrue()
//    {
//        // Arrange
//        var invoiceId = 1;
//        _invoiceRepositoryMock.Setup(r => r.DeleteById(invoiceId))
//            .ReturnsAsync(true);
//        _unitOfWorkMock.Setup(u => u.CompleteAsync())
//            .ReturnsAsync(1);

//        // Act
//        var result = await _invoiceService.Delete_Invoice(invoiceId);

//        // Assert
//        result.IsSuccess.Should().BeTrue();
//        result.Value.Should().BeTrue();
//        _invoiceRepositoryMock.Verify(r => r.DeleteById(invoiceId), Times.Once);
//        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
//    }

//    [Fact]
//    public async Task DeleteInvoice_InvoiceNotExist_ReturnFalse()
//    {
//        // Arrange
//        var invoiceId = 1;
//        _invoiceRepositoryMock.Setup(r => r.DeleteById(invoiceId))
//            .ReturnsAsync(false);

//        // Act
//        var result = await _invoiceService.Delete_Invoice(invoiceId);

//        // Assert
//        result.IsSuccess.Should().BeFalse();
//        result.Error.Title.Should().Be("Invoice not found");
//        result.Error.StatusCode.Should().Be(404);
//        _invoiceRepositoryMock.Verify(r => r.DeleteById(invoiceId), Times.Once);
//        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
//    }

//    [Fact]
//    public async Task DownloadInvoice_InvalidInvoiceId_ReturnsBadRequest()
//    {
//        // Act
//        var result = await _invoiceService.Download_Invoice(0);

//        // Assert
//        result.IsSuccess.Should().BeFalse();
//        result.Error.Title.Should().Be("invalid invoice id");
//        result.Error.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
//    }

//    [Fact]
//    public async Task DownloadInvoice_InvoiceNotExist_ReturnsNotFound()
//    {
//        // Arrange
//        var invoiceId = 1;
//        _invoiceRepositoryMock.Setup(r => r.GetByIdAsync(invoiceId))
//            .ReturnsAsync((Invoice)null);

//        // Act
//        var result = await _invoiceService.Download_Invoice(invoiceId);

//        // Assert
//        result.IsSuccess.Should().BeFalse();
//        result.Error.Title.Should().Be("Invoice not found");
//        result.Error.StatusCode.Should().Be(404);
//    }

//    [Fact]
//    public async Task DownloadInvoice_ValidInvoice_ReturnsFileResult()
//    {
//        // Arrange
//        var invoice = new Invoice
//        {
//            Id = 1,
//            InvoiceNumber = "INV-123",
//            OrderDate = DateTime.UtcNow,
//            TotalAmount = 199.99m,
//            UserEmail = "test@example.com",
//            CreatedAt = DateTime.UtcNow,
//            OrderId = 456
//        };

//        _invoiceRepositoryMock.Setup(r => r.GetByIdAsync(1))
//            .ReturnsAsync(invoice);

//        // Act
//        var result = await _invoiceService.Download_Invoice(1);

//        // Assert
//        result.IsSuccess.Should().BeTrue();
//        var fileResult = result.Value as FileContentResult;
//        fileResult.Should().NotBeNull();
//        fileResult.ContentType.Should().Be("application/pdf");
//        fileResult.FileDownloadName.Should().Be($"Invoice_{invoice.InvoiceNumber}.pdf");
//        fileResult.FileContents.Should().NotBeEmpty();
//    }

//    [Fact]
//    public async Task DownloadInvoice_PdfGenerationFailure_ReturnsServerError()
//    {
//        // Arrange
//        var invalidInvoice = new Invoice
//        {
//            Id = 1,
//            InvoiceNumber = null, // Will cause NRE in QuestPDF
//            OrderDate = DateTime.UtcNow
//        };

//        _invoiceRepositoryMock.Setup(r => r.GetByIdAsync(1))
//            .ReturnsAsync(invalidInvoice);

//        // Act
//        var result = await _invoiceService.Download_Invoice(1);

//        // Assert
//        result.IsSuccess.Should().BeFalse();
//        result.Error.Title.Should().Be("PDF Generation Failed");
//        result.Error.StatusCode.Should().Be(500);
//    }
//    [Fact]
//    public async Task Get_Invoices_UserNotFound_ReturnsFailure()
//    {
//        // Arrange
//        var userId = "invalid_user";
//        _userManagerMock.Setup(um => um.FindByIdAsync(userId))
//            .ReturnsAsync((AppUser)null);

//        // Act
//        var result = await _invoiceService.Get_Invoices(userId);

//        // Assert
//        result.IsSuccess.Should().BeFalse();
//        result.Error.Should().Be(404);
//        _userManagerMock.Verify(um => um.FindByIdAsync(userId), Times.Once);
//    }

//    [Fact]
//    public async Task Get_Invoices_EmptyResult_ReturnsEmptyList()
//    {
//        // Arrange
//        var userId = "valid_user";
//        var user = new AppUser { Id = userId, Email = "test@example.com" };
//        var emptyList = new List<Invoice>().AsReadOnly();

//        _userManagerMock.Setup(um => um.FindByIdAsync(userId))
//            .ReturnsAsync(user);
//        _invoiceRepositoryMock.Setup(r => r.GetAllWithSpecAsync(It.IsAny<InvoiceSpecifications>()))
//            .ReturnsAsync(emptyList);

//        // Act
//        var result = await _invoiceService.Get_Invoices(userId);

//        // Assert
//        result.IsSuccess.Should().BeTrue();
//        result.Value.Should().BeEmpty();
//        _invoiceRepositoryMock.Verify(r => r.GetAllWithSpecAsync(
//            It.Is<InvoiceSpecifications>(s => s.Email == user.Email)
//        ), Times.Once);
//    }

//    [Fact]
//    public async Task Get_Invoices_WithResults_ReturnsInvoiceList()
//    {
//        // Arrange
//        var userId = "valid_user";
//        var user = new AppUser { Id = userId, Email = "user@example.com" };
//        var invoices = new List<Invoice>
//    {
//        new Invoice { UserEmail = user.Email },
//        new Invoice { UserEmail = user.Email }
//    }.AsReadOnly();

//        _userManagerMock.Setup(um => um.FindByIdAsync(userId))
//            .ReturnsAsync(user);
//        _invoiceRepositoryMock.Setup(r => r.GetAllWithSpecAsync(
//            It.Is<InvoiceSpecifications>(s => s.Email == user.Email)
//        )).ReturnsAsync(invoices);

//        // Act
//        var result = await _invoiceService.Get_Invoices(userId);

//        // Assert
//        result.IsSuccess.Should().BeTrue();
//        result.Value.Should().HaveCount(2);
//        result.Value.Should().OnlyContain(i => i.UserEmail == user.Email);
//    }

//    [Fact]
//    public async Task Get_Invoices_DatabaseError_ReturnsFailure()
//    {
//        // Arrange
//        var userId = "valid_user";
//        var user = new AppUser { Id = userId, Email = "test@example.com" };

//        _userManagerMock.Setup(um => um.FindByIdAsync(userId))
//            .ReturnsAsync(user);
//        _invoiceRepositoryMock.Setup(r => r.GetAllWithSpecAsync(It.IsAny<InvoiceSpecifications>()))
//            .ThrowsAsync(new Exception("Database failure"));

//        // Act
//        var result = await _invoiceService.Get_Invoices(userId);

//        // Assert
//        result.IsSuccess.Should().BeFalse();
//        result.Error.Should().Be(500);
//    }

//    [Fact]
//    public async Task Get_Invoices_NullUserId_ReturnsUserNotFound()
//    {
//        // Act
//        var result = await _invoiceService.Get_Invoices(null);

//        // Assert
//        result.IsSuccess.Should().BeFalse();
//        result.Error.Should().Be(404);
//    }
 

//        [Fact]
//        public async Task Filter_Invoices_ValidParams_ReturnsFilteredInvoices()
//        {
//            // Arrange
//            var param = new InvoiceSpecificationsParams
//            {
//                PageIndex = 1,
//                PageSize = 5,
//                Sort = "date_asc",
//                SearchTerm = "John",
//                OrderId = 123
//            };

//            var expectedInvoices = new List<Invoice>
//            {
//                new Invoice { Id = 1, UserName = "John" },
//                new Invoice { Id = 2, UserName = "John" }
//            }.AsReadOnly();

//        _invoiceRepositoryMock.Setup(r => r.GetAllWithSpecAsync(It.IsAny<InvoiceSpecifications>()))
//                .ReturnsAsync(expectedInvoices);

//            // Act
//            var result = await _invoiceService.Filter_Invoices(param);

//            // Assert
//            result.IsSuccess.Should().BeTrue();
//            result.Value.Should().HaveCount(2);
//            _unitOfWorkMock.Verify(u => u.Repository<Invoice>(), Times.Once);
//        }

//        [Fact]
//        public async Task Filter_Invoices_Pagination_LimitsPageSize()
//        {
//            // Arrange
//            var param = new InvoiceSpecificationsParams
//            {
//                PageSize = 15  // Exceeds MaxPageSize of 10
//            };

//            // Act
//            await _invoiceService.Filter_Invoices(param);

//            // Assert
//            param.PageSize.Should().Be(10);
//        }

//        [Fact]
//        public async Task Filter_Invoices_SearchByName_FiltersCorrectly()
//        {
//            // Arrange
//            var param = new InvoiceSpecificationsParams
//            {
//                SearchTerm = "Smith"
//            };

//            var expectedInvoices = new List<Invoice>
//            {
//                new Invoice { UserName = "Smith" }
//            }.AsReadOnly();

//        //_invoiceRepositoryMock.Setup(r => r.GetAllWithSpecAsync(It.Is<InvoiceSpecifications>(s =>
//        //        s.SearchTerm == "Smith")))
//        //        .ReturnsAsync(expectedInvoices);

//            var result = await _invoiceService.Filter_Invoices(param);

//            result.Value.Should().OnlyContain(i => i.UserName == "Smith");
//        }

//        [Fact]
//        public async Task Filter_Invoices_OrderIdFilter_AppliesCorrectly()
//        {
//            var param = new InvoiceSpecificationsParams
//            {
//                OrderId = 456
//            };

//            var expectedInvoices = new List<Invoice>
//            {
//                new Invoice { OrderId = 456 }
//            }.AsReadOnly();

//        //_invoiceRepositoryMock.Setup(r => r.GetAllWithSpecAsync(It.Is<InvoiceSpecifications>(s =>
//        //        s.OrderId == 456)))
//        //        .ReturnsAsync(expectedInvoices);

//            var result = await _invoiceService.Filter_Invoices(param);

//            // Assert
//            result.Value.Should().OnlyContain(i => i.OrderId == 456);
//        }

//        [Fact]
//        public async Task Filter_Invoices_DatabaseError_ReturnsFailure()
//        {
//            // Arrange
//            var param = new InvoiceSpecificationsParams();
//            var errorMessage = "Database connection failed";

//        _invoiceRepositoryMock.Setup(r => r.GetAllWithSpecAsync(It.IsAny<InvoiceSpecifications>()))
//                .ThrowsAsync(new Exception(errorMessage));

//            // Act
//            var result = await _invoiceService.Filter_Invoices(param);

//            // Assert
//            result.IsSuccess.Should().BeFalse();
//            result.Error.Should().Be(500);
//        }

//        [Fact]
//        public async Task Filter_Invoices_DefaultParams_ReturnsAllInvoices()
//        {
//            // Arrange
//            var param = new InvoiceSpecificationsParams();
//            var allInvoices = new List<Invoice>(10).AsReadOnly();

//        _invoiceRepositoryMock.Setup(r => r.GetAllWithSpecAsync(It.IsAny<InvoiceSpecifications>()))
//                .ReturnsAsync(allInvoices);

//            // Act
//            var result = await _invoiceService.Filter_Invoices(param);

//            // Assert
//            result.IsSuccess.Should().BeTrue();
//            result.Value.Should().HaveCount(10);
//        }
    


//[Fact]
//    public async Task Get_CurrentUserInvoices_ValidUser_ReturnsInvoices()
//    {
//        // Arrange
//        var userId = "user123";
//        var userEmail = "test@example.com";
//        var invoices = new List<Invoice>
//    {
//        new Invoice { UserEmail = userEmail },
//        new Invoice { UserEmail = userEmail }
//    }.AsReadOnly();

//        _userManagerMock.Setup(um => um.FindByIdAsync(userId))
//            .ReturnsAsync(new AppUser { Id = userId, Email = userEmail });
//        _invoiceRepositoryMock.Setup(r => r.GetAllWithSpecAsync(It.IsAny<InvoiceSpecifications>()))
//            .ReturnsAsync(invoices);

//        // Act
//        var result = await _invoiceService.Get_CurrentUserInvoices();

//        // Assert
//        result.IsSuccess.Should().BeTrue();
//        result.Value.Should().HaveCount(2);
//        result.Value.Should().OnlyContain(i => i.UserEmail == userEmail);
//    }

//    [Fact]
//    public async Task Get_CurrentUserInvoices_UserNotFound_ReturnsFailure()
//    {
//        // Arrange
//        var invalidUserId = "invalid_user";

//        //_mockUserService.Setup(u => u.GetCurrentUserId())
//        //    .Returns(invalidUserId);

//        // Mock user lookup failure
//        _userManagerMock.Setup(um => um.FindByIdAsync(invalidUserId))
//            .ReturnsAsync((AppUser)null);

//        // Act
//        var result = await _invoiceService.Get_CurrentUserInvoices();

//        // Assert
//        result.IsSuccess.Should().BeFalse();
//        result.Error.Should().Be(404);
//        //_mockUserService.Verify(u => u.GetCurrentUserId(), Times.Once);
//    }

//    [Fact]
//    public async Task Get_CurrentUserInvoices_DatabaseError_ReturnsFailure()
//    {
//        // Arrange
//        var userId = "user123";
//        var errorMessage = "Database failure";

//        // Mock valid user ID
//        //_mockUserService.Setup(u => u.GetCurrentUserId())
//        //    .Returns(userId);

//        // Mock valid user lookup
//        _userManagerMock.Setup(um => um.FindByIdAsync(userId))
//            .ReturnsAsync(new AppUser { Id = userId, Email = "test@example.com" });

//        // Force repository failure
//        _invoiceRepositoryMock.Setup(r => r.GetAllWithSpecAsync(It.IsAny<InvoiceSpecifications>()))
//            .ThrowsAsync(new Exception(errorMessage));

//        // Act
//        var result = await _invoiceService.Get_CurrentUserInvoices();

//        // Assert
//        result.IsSuccess.Should().BeFalse();
//        result.Error.Should().Be(500);
//    }
//    [Fact]
//    public async Task GenerateInvoiceNumber_NoInvoices_ReturnsDefaultNumber()
//    {
//        // Arrange
//        _invoiceRepositoryMock.Setup(r => r.GetLastOrDefaultAsync())
//            .ReturnsAsync((Invoice)null);

//        // Act
//        var result = await _invoiceService.Generate_Invoice_Number_Automatically();

//        // Assert
//        result.IsSuccess.Should().BeTrue();
//        result.Value.Should().Be("INV-0001");
//        _invoiceRepositoryMock.Verify(r => r.GetLastOrDefaultAsync(), Times.Once);
//    }

//    [Fact]
//    public async Task GenerateInvoiceNumber_ValidLastInvoice_ReturnsIncrementedNumber()
//    {
//        // Arrange
//        var lastInvoice = new Invoice { InvoiceNumber = "INV-0003" };
//        _invoiceRepositoryMock.Setup(r => r.GetLastOrDefaultAsync())
//            .ReturnsAsync(lastInvoice);

//        // Act
//        var result = await _invoiceService.Generate_Invoice_Number_Automatically();

//        // Assert
//        result.IsSuccess.Should().BeTrue();
//        result.Value.Should().Be("INV-0004");
//    }

//    [Fact]
//    public async Task GenerateInvoiceNumber_LastInvoiceWithInvalidNumber_ReturnsDefaultNumber()
//    {
//        // Arrange
//        var lastInvoice = new Invoice { InvoiceNumber = "INV-ABC" };
//        _invoiceRepositoryMock.Setup(r => r.GetLastOrDefaultAsync())
//            .ReturnsAsync(lastInvoice);

//        // Act
//        var result = await _invoiceService.Generate_Invoice_Number_Automatically();

//        // Assert
//        result.IsSuccess.Should().BeTrue();
//        result.Value.Should().Be("INV-0001");
//    }

//    [Fact]
//    public async Task GenerateInvoiceNumber_LastInvoiceWithNullNumber_ReturnsDefaultNumber()
//    {
//        // Arrange
//        var lastInvoice = new Invoice { InvoiceNumber = null! };
//        _invoiceRepositoryMock.Setup(r => r.GetLastOrDefaultAsync())
//            .ReturnsAsync(lastInvoice);

//        // Act
//        var result = await _invoiceService.Generate_Invoice_Number_Automatically();

//        // Assert
//        result.IsSuccess.Should().BeTrue();
//        result.Value.Should().Be("INV-0001");
//    }

//    [Fact]
//    public async Task GenerateInvoiceNumber_DatabaseError_ReturnsFailure()
//    {
//        // Arrange
//        _invoiceRepositoryMock.Setup(r => r.GetLastOrDefaultAsync())
//            .ThrowsAsync(new Exception("Database connection failed"));

//        // Act
//        var result = await _invoiceService.Generate_Invoice_Number_Automatically();

//        // Assert
//        result.IsSuccess.Should().BeFalse();
//        result.Error.Should().Be(500);
//    }

//    [Fact]
//    public async Task GenerateInvoiceNumber_NumberRollover_HandlesLargeNumbers()
//    {
//        // Arrange
//        var lastInvoice = new Invoice { InvoiceNumber = "INV-9999" };
//        _invoiceRepositoryMock.Setup(r => r.GetLastOrDefaultAsync())
//            .ReturnsAsync(lastInvoice);

//        // Act
//        var result = await _invoiceService.Generate_Invoice_Number_Automatically();

//        // Assert
//        result.IsSuccess.Should().BeTrue();
//        result.Value.Should().Be("INV-10000");
//    }
//    [Fact]
//    public async Task GetAllInvoicesAsync_WithInvoices_ReturnsList()
//    {
//        // Arrange
//        var invoices = new List<Invoice>
//    {
//        new Invoice { Id = 1, TotalAmount = 100 },
//        new Invoice { Id = 2, TotalAmount = 200 }
//    }.AsReadOnly();

//        _invoiceRepositoryMock
//            .Setup(r => r.GetAllWithSpecAsync(It.IsAny<InvoiceSpecifications>()))
//            .ReturnsAsync(invoices);

//        // Act
//        var result = await _invoiceService.GetAllInvoicesAsync();

//        // Assert
//        result.IsSuccess.Should().BeTrue();
//        result.Value.Should().HaveCount(2);
//        result.Value.Should().BeEquivalentTo(invoices);
//        _invoiceRepositoryMock.Verify(
//            r => r.GetAllWithSpecAsync(It.IsAny<InvoiceSpecifications>()),
//            Times.Once
//        );
//    }

//    [Fact]
//    public async Task GetAllInvoicesAsync_Empty_ReturnsEmptyList()
//    {
//        // Arrange
//        var emptyInvoices = new List<Invoice>().AsReadOnly();

//        _invoiceRepositoryMock
//            .Setup(r => r.GetAllWithSpecAsync(It.IsAny<InvoiceSpecifications>()))
//            .ReturnsAsync(emptyInvoices);

//        // Act
//        var result = await _invoiceService.GetAllInvoicesAsync();

//        // Assert
//        result.IsSuccess.Should().BeTrue();
//        result.Value.Should().BeEmpty();
//    }

//    [Fact]
//    public async Task GetAllInvoicesAsync_DatabaseError_ReturnsFailure()
//    {
//        // Arrange
//        var errorMessage = "Database connection failed";

//        _invoiceRepositoryMock
//            .Setup(r => r.GetAllWithSpecAsync(It.IsAny<InvoiceSpecifications>()))
//            .ThrowsAsync(new Exception(errorMessage));

//        // Act
//        var result = await _invoiceService.GetAllInvoicesAsync();

//        // Assert
//        result.IsSuccess.Should().BeFalse();
//        result.Error.Should().Be(500);
//        result.Error.StatusCode.Should().Be(500);
//    }
//    [Fact]
//    public async Task PayInvoice_InvalidId_ReturnFalse()
//    {
//        // Arrange
//        var invalidInvoiceId = 0;
//        var paymentMethod = "CreditCard";

//        // Act
//        var result = await _invoiceService.Pay_Invoice(invalidInvoiceId, paymentMethod);

//        // Assert
//        result.IsSuccess.Should().BeFalse();
//        result.Error.Title.Should().Be("Invoice not found");
//        result.Error.StatusCode.Should().Be(404);
//    }

//    [Fact]
//    public async Task PayInvoice_NotFoundInvoice_ReturnFalse()
//    {
//        // Arrange
//        var invoiceId = 1;
//        _invoiceRepositoryMock.Setup(r => r.GetByIdAsync(invoiceId))
//            .ReturnsAsync((Invoice)null);

//        // Act
//        var result = await _invoiceService.Pay_Invoice(invoiceId, "CreditCard");

//        // Assert
//        result.IsSuccess.Should().BeFalse();
//        result.Error.Title.Should().Be("Invoice not found");
//        result.Error.StatusCode.Should().Be(404);
//    }

//    [Fact]
//    public async Task PayInvoice_PaidInvoice_ReturnFalse()
//    {
//        // Arrange
//        var invoiceId = 1;
//        var paidInvoice = new Invoice { Id = invoiceId, IsPaid = true };
//        _invoiceRepositoryMock.Setup(r => r.GetByIdAsync(invoiceId))
//            .ReturnsAsync(paidInvoice);

//        // Act
//        var result = await _invoiceService.Pay_Invoice(invoiceId, "CreditCard");

//        // Assert
//        result.IsSuccess.Should().BeFalse();
//        result.Error.Title.Should().Be("Invoice already paid");
//        result.Error.StatusCode.Should().Be(400);
//    }

//    [Fact]
//    public async Task PayInvoice_ValidInput_ReturnSuccess()
//    {
//        // Arrange
//        var invoiceId = 1;
//        var paymentMethod = "PayPal";
//        var invoice = new Invoice
//        {
//            Id = invoiceId,
//            IsPaid = false,
//            PaymentMethod = null,
//        };

//        _invoiceRepositoryMock.Setup(r => r.GetByIdAsync(invoiceId))
//            .ReturnsAsync(invoice);
//        _unitOfWorkMock.Setup(u => u.CompleteAsync())
//            .ReturnsAsync(1);

//        // Act
//        var result = await _invoiceService.Pay_Invoice(invoiceId, paymentMethod);

//        // Assert
//        result.IsSuccess.Should().BeTrue();
//        invoice.IsPaid.Should().BeTrue();
//        invoice.PaymentMethod.Should().Be(paymentMethod);
//        invoice.PaymentDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(1));
//        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
//    }

//    // Note: This test will fail until payment method validation is implemented
//    [Fact]
//    public async Task PayInvoice_InvalidPaymentMethod_ReturnFalse()
//    {
//        // Arrange
//        var invoiceId = 1;
//        var invoice = new Invoice { Id = invoiceId, IsPaid = false };
//        _invoiceRepositoryMock.Setup(r => r.GetByIdAsync(invoiceId))
//            .ReturnsAsync(invoice);

//        // Act
//        var result = await _invoiceService.Pay_Invoice(invoiceId, "");

//        // Assert
//        result.IsSuccess.Should().BeFalse();
//        result.Error.Title.Should().Be("Invalid payment method");
//    }
//    [Fact]
//    public async Task Send_Invoice_InvalidInvoiceId_ReturnsBadRequest()
//    {
//        // Arrange
//        var invalidInvoiceId = 0;

//        // Act
//        var result = await _invoiceService.Send_Invoice(invalidInvoiceId);

//        // Assert
//        result.IsSuccess.Should().BeFalse();
//        result.Error.Title.Should().Be("invalid invoice id ");
//        result.Error.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
//    }

//    [Fact]
//    public async Task Send_Invoice_InvoiceNotFound_ReturnsNotFound()
//    {
//        // Arrange
//        var invoiceId = 1;
//        _invoiceRepositoryMock.Setup(r => r.GetByIdAsync(invoiceId))
//            .ReturnsAsync((Invoice)null);

//        // Act
//        var result = await _invoiceService.Send_Invoice(invoiceId);

//        // Assert
//        result.IsSuccess.Should().BeFalse();
//        result.Error.Title.Should().Be("invoice not found ");
//        result.Error.StatusCode.Should().Be(StatusCodes.Status404NotFound);
//    }

//    [Fact]
//    public async Task Send_Invoice_UserNotFound_ReturnsBadRequest()
//    {
//        // Arrange
//        var invoiceId = 1;
//        var invoice = new Invoice { Id = invoiceId, UserEmail = "test@example.com" };

//        _invoiceRepositoryMock.Setup(r => r.GetByIdAsync(invoiceId))
//            .ReturnsAsync(invoice);
//        _userManagerMock.Setup(um => um.FindByEmailAsync(invoice.UserEmail))
//            .ReturnsAsync((AppUser)null);

//        // Act
//        var result = await _invoiceService.Send_Invoice(invoiceId);

//        // Assert
//        result.IsSuccess.Should().BeFalse();
//        result.Error.Title.Should().Be("user  not found ");
//        result.Error.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
//    }

//    [Fact]
//    public async Task Send_Invoice_ValidRequest_SendsEmailAndReturnsInvoice()
//    {
//        // Arrange
//        var invoiceId = 1;
//        var userEmail = "test@example.com";
//        var invoice = new Invoice
//        {
//            Id = invoiceId,
//            UserEmail = userEmail,
//            TotalAmount = 199.99m,
//            OrderDate = DateTime.UtcNow
//        };
//        var user = new AppUser { Email = userEmail };

//        _invoiceRepositoryMock.Setup(r => r.GetByIdAsync(invoiceId))
//            .ReturnsAsync(invoice);
//        _userManagerMock.Setup(um => um.FindByEmailAsync(userEmail))
//            .ReturnsAsync(user);
//        //_emailSenderServiceMock.Setup(e => e.SendEmailAsync(
//        //    userEmail,
//        //    "فاتورتك من Cosmatic Store",
//        //    It.IsAny<string>()
//        //)).Returns(Task.CompletedTask);

//        var result = await _invoiceService.Send_Invoice(invoiceId);

//        result.IsSuccess.Should().BeTrue();
//        result.Value.Should().BeEquivalentTo(invoice);

//        //_emailSenderServiceMock.Verify(e => e.SendEmailAsync(
//        //    userEmail,
//        //    "فاتورتك من Cosmatic Store",
//        //    It.Is<string>(body =>
//        //        body.Contains($"فاتورة رقم {invoiceId}") &&
//        //        body.Contains($"{invoice.TotalAmount} ج.م") &&
//        //        body.Contains(invoice.OrderDate.ToString())
//        //    )
//        //), Times.Once);
   
//    }

//    [Fact]
//    public async Task Send_Invoice_DatabaseFailure_ReturnsError()
//    {
//        // Arrange
//        var invoiceId = 1;
//        _invoiceRepositoryMock.Setup(r => r.GetByIdAsync(invoiceId))
//            .ThrowsAsync(new Exception("Database failure"));

//        // Act
//        var result = await _invoiceService.Send_Invoice(invoiceId);

//        // Assert
//        result.IsSuccess.Should().BeFalse();
//        result.Error.Title.Should().Be("Database Error");
//        result.Error.Message.Should().Be("Database failure");
//    }
//    [Fact]
//    public async Task GetInvoiceById_ExistingInvoice_ReturnsInvoice()
//    {
//        // Arrange
//        var invoiceId = 1;
//        var expectedInvoice = new Invoice { Id = invoiceId };
//        _invoiceRepositoryMock.Setup(r => r.GetByIdAsync(invoiceId))
//            .ReturnsAsync(expectedInvoice);

//        // Act
//        var result = await _invoiceService.Get_invoice_by_id(invoiceId);

//        // Assert
//        result.IsSuccess.Should().BeTrue();
//        result.Value.Should().BeEquivalentTo(expectedInvoice);
//        _invoiceRepositoryMock.Verify(r => r.GetByIdAsync(invoiceId), Times.Once);
//    }

//    [Fact]
//    public async Task GetInvoiceById_NotFound_ReturnsError()
//    {
//        // Arrange
//        var invoiceId = 1;
//        _invoiceRepositoryMock.Setup(r => r.GetByIdAsync(invoiceId))
//            .ReturnsAsync((Invoice)null);

//        // Act
//        var result = await _invoiceService.Get_invoice_by_id(invoiceId);

//        // Assert
//        result.IsSuccess.Should().BeFalse();
//        result.Error.Title.Should().Be("Invoice not found");
//        result.Error.StatusCode.Should().Be(404);
//        _invoiceRepositoryMock.Verify(r => r.GetByIdAsync(invoiceId), Times.Once);
//    }

//    [Fact]
//    public async Task GetInvoiceById_DatabaseError_ReturnsFailure()
//    {
//        // Arrange
//        var invoiceId = 1;
//        var errorMessage = "Database connection failed";
//        _invoiceRepositoryMock.Setup(r => r.GetByIdAsync(invoiceId))
//            .ThrowsAsync(new Exception(errorMessage));

//        // Act
//        var result = await _invoiceService.Get_invoice_by_id(invoiceId);

//        // Assert
//        result.IsSuccess.Should().BeFalse();
//        result.Error.Title.Should().Be("Database Error");
//        result.Error.Message.Should().Be(errorMessage);
//        _invoiceRepositoryMock.Verify(r => r.GetByIdAsync(invoiceId), Times.Once);
//    }
//    [Fact]
//    public async Task TrackInvoiceStatus_InvalidInvoiceId_ReturnsBadRequest()
//    {
//        // Act
//        var result = await _invoiceService.Track_Invoice_Status(0);

//        // Assert
//        result.IsSuccess.Should().BeFalse();
//        result.Error.Title.Should().Be("invalid invoice id");
//        result.Error.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
//    }

//    [Fact]
//    public async Task TrackInvoiceStatus_InvoiceNotFound_ReturnsNotFound()
//    {
//        // Arrange
//        var invoiceId = 1;
//        _invoiceRepositoryMock.Setup(r => r.GetByIdAsync(invoiceId))
//            .ReturnsAsync((Invoice)null);

//        // Act
//        var result = await _invoiceService.Track_Invoice_Status(invoiceId);

//        // Assert
//        result.IsSuccess.Should().BeFalse();
//        result.Error.Title.Should().Be("Invoice not found");
//        result.Error.StatusCode.Should().Be(404);
//    }

//    [Fact]
//    public async Task TrackInvoiceStatus_PaidInvoice_ReturnsPaidStatus()
//    {
//        // Arrange
//        var invoiceId = 1;
//        var invoice = new Invoice { Id = invoiceId, IsPaid = true };
//        _invoiceRepositoryMock.Setup(r => r.GetByIdAsync(invoiceId))
//            .ReturnsAsync(invoice);

//        // Act
//        var result = await _invoiceService.Track_Invoice_Status(invoiceId);

//        // Assert
//        result.IsSuccess.Should().BeTrue();
//        result.Value.Should().Contain("مدفوعة");
//    }

//    [Fact]
//    public async Task TrackInvoiceStatus_UnpaidWithStatus_ReturnsStatusText()
//    {
//        // Arrange
//        var invoiceId = 1;
//        var invoice = new Invoice
//        {
//            Id = invoiceId,
//            IsPaid = false,
//            InvoiceStatus = Status.Pending
//        };
//        _invoiceRepositoryMock.Setup(r => r.GetByIdAsync(invoiceId))
//            .ReturnsAsync(invoice);

//        // Act
//        var result = await _invoiceService.Track_Invoice_Status(invoiceId);

//        // Assert
//        result.IsSuccess.Should().BeTrue();
//        result.Value.Should().Contain("Pending");
//    }

//    [Fact]
//    public async Task TrackInvoiceStatus_DatabaseError_ReturnsFailure()
//    {
//        // Arrange
//        var invoiceId = 1;
//        _invoiceRepositoryMock.Setup(r => r.GetByIdAsync(invoiceId))
//            .ThrowsAsync(new Exception("Database error"));

//        // Act
//        var result = await _invoiceService.Track_Invoice_Status(invoiceId);

//        // Assert
//        result.IsSuccess.Should().BeFalse();
//        result.Error.Title.Should().Be("Database Error");
//        result.Error.Message.Should().Contain("Database error");
//    }
//    [Fact]
//    public async Task Update_Invoice_Successful_ReturnsSuccess()
//    {
//        // Arrange
//        var invoice = new Invoice { Id = 1 };
//        _unitOfWorkMock.Setup(u => u.CompleteAsync())
//            .ReturnsAsync(1);

//        // Act
//        var result = await _invoiceService.Update_Invoice(invoice);

//        // Assert
//        result.IsSuccess.Should().BeTrue();
//        _invoiceRepositoryMock.Verify(r => r.Update(invoice), Times.Once);
//        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
//    }

//    [Fact]
//    public async Task Update_Invoice_DatabaseUpdateFailure_ReturnsError()
//    {
//        // Arrange
//        var invoice = new Invoice { Id = 1 };
//        _invoiceRepositoryMock.Setup(r => r.Update(invoice))
//            .Throws(new Exception("Update failed"));

//        // Act
//        var result = await _invoiceService.Update_Invoice(invoice);

//        // Assert
//        result.IsSuccess.Should().BeFalse();
//        result.Error.Should().Be(500);
//        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
//    }

//    [Fact]
//    public async Task Update_Invoice_DatabaseSaveFailure_ReturnsError()
//    {
//        // Arrange
//        var invoice = new Invoice { Id = 1 };
//        _unitOfWorkMock.Setup(u => u.CompleteAsync())
//            .ThrowsAsync(new Exception("Save failed"));

//        // Act
//        var result = await _invoiceService.Update_Invoice(invoice);

//        // Assert
//        result.IsSuccess.Should().BeFalse();
//        result.Error.Should().Be(500);
//        _invoiceRepositoryMock.Verify(r => r.Update(invoice), Times.Once);
//    }

//    [Fact]
//    public async Task Update_Invoice_NullInput_ReturnsDatabaseError()
//    {
//        // Arrange
//        Invoice invalidInvoice = null!;
//        //_invoiceRepositoryMock.Setup(r => r.Update(It.Is<Invoice>(i => i == null!))
//        //    .Throws(new ArgumentNullException(nameof(invalidInvoice)));

//        // Act
//        var result = await _invoiceService.Update_Invoice(invalidInvoice);

//        // Assert
//        result.IsSuccess.Should().BeFalse();
//        result.Error.Title.Should().Be("Database Error");
//        result.Error.Message.Should().Contain("Value cannot be null");
//        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
//    }
//}

