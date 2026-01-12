using E_Commerce.Application.Services.Core;
using E_Commerce.Infrastructure.Persistence.Seeding.ExcelParser;
using E_Commerce.Repository.Repositories.Interfaces;
using E_Commerce.Repository.Specifications.ProductSpecifications;

namespace E_Commerce.UnitTesting.Services;

public class ProductServiceTest
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IImportProductData> _mockImportData;
    private readonly Mock<IGenericRepository<Product>> _mockProductRepo;
    private readonly ProductService _productService;

    public ProductServiceTest()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockProductRepo = new Mock<IGenericRepository<Product>>();
        _mockImportData = new Mock<IImportProductData>();

        _mockUnitOfWork.Setup(uow => uow.Repository<Product>())
            .Returns(_mockProductRepo.Object);

        _productService = new ProductService(_mockUnitOfWork.Object, _mockImportData.Object);
    }

    [Fact]
    public async Task CreateProductAsync_ProductIsNull_ReturnsFailure()
    {
        var result = await _productService.CreateProductAsync(null);

        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(404);
        result.Error.Title.Should().Be("Product cannot be null");
        result.Error.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task CreateProductAsync_ValidProduct_ReturnsSuccess()
    {
        var product = new Product();
        _mockUnitOfWork.Setup(uow => uow.CompleteAsync())
            .ReturnsAsync(1);  
        var result = await _productService.CreateProductAsync(product);
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Product created successfully");
        _mockProductRepo.Verify(repo => repo.AddAsync(product), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_NoChangesPersisted_ReturnsFailure()
    {
        var product = new Product();
        _mockUnitOfWork.Setup(uow => uow.CompleteAsync())
            .ReturnsAsync(0);  
        var result = await _productService.CreateProductAsync(product);
        result.IsSuccess.Should().BeFalse();
        result.Error.Title.Should().Be("No changes persisted");
    }

    [Fact]
    public async Task CreateProductAsync_DatabaseThrowsException_ReturnsError()
    {
        // Arrange
        var product = new Product();
        var exceptionMessage = "Database failure";
        _mockUnitOfWork.Setup(uow => uow.CompleteAsync())
            .ThrowsAsync(new Exception(exceptionMessage));

        var result = await _productService.CreateProductAsync(product);

        result.IsSuccess.Should().BeFalse();
        result.Error.Title.Should().Be(exceptionMessage);
        result.Error.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task CreateProductAsync_AddProductThrowsException_ReturnsError()
    {
        var product = new Product();
        var exceptionMessage = "Add failed";
        _mockProductRepo.Setup(repo => repo.AddAsync(product))
            .ThrowsAsync(new Exception(exceptionMessage));
        var result = await _productService.CreateProductAsync(product);
        result.IsSuccess.Should().BeFalse();
        result.Error.Title.Should().Be(exceptionMessage);
        result.Error.StatusCode.Should().Be(500);
    }
 

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetProductByIdAsync_InvalidId_ReturnsFailure(int invalidId)
    {
        // Act
        var result = await _productService.GetProductByIdAsync(invalidId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Title.Should().Be("Invalid product ID");
        result.Error.StatusCode.Should().Be(400);

        _mockProductRepo.Verify(repo =>
            repo.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetProductByIdAsync_ValidId_ProductExists_ReturnsSuccess()
    {
        // Arrange
        var testProduct = new Product { Id = 1, Name = "Test Product" };
        _mockProductRepo.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(testProduct);

        // Act
        var result = await _productService.GetProductByIdAsync(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(testProduct);
        _mockProductRepo.Verify(repo =>
            repo.GetByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetProductByIdAsync_ValidId_ProductNotFound_ReturnsFailure()
    {
        // Arrange
        _mockProductRepo.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync((Product)null);

        // Act
        var result = await _productService.GetProductByIdAsync(1);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(404);
        result.Error.Title.Should().Be("Product with ID 1 not found");
        _mockProductRepo.Verify(repo =>
            repo.GetByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetProductByIdAsync_DatabaseError_ReturnsFailure()
    {
        // Arrange
        var exceptionMessage = "Database connection failed";
        _mockProductRepo.Setup(repo => repo.GetByIdAsync(1))
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _productService.GetProductByIdAsync(1);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(500);
        result.Error.Title.Should().Be(exceptionMessage);
        _mockProductRepo.Verify(repo =>
            repo.GetByIdAsync(1), Times.Once);
    }

  

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(-1, 5)]
    [InlineData(5, -2)]
    public async Task GetAllProductsAsync_InvalidPagination_ReturnsFailure(int pageSize, int pageIndex)
    {
        // Act
        var result = await _productService.GetAllProductsAsync(pageSize, pageIndex);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Title.Should().Be("Page size and index must be greater than 0");
        result.Error.StatusCode.Should().Be(400);

        _mockProductRepo.Verify(repo =>
            repo.GetAllWithSpecAsync(It.IsAny<ProductSpecification>()), Times.Never);
        _mockImportData.Verify(m => m.ImportProductsFromExcel(), Times.Never);
    }

    [Fact]
    public async Task GetAllProductsAsync_ProductsExist_ReturnsProducts()
    {
        // Arrange
        var testProducts = new List<Product> { new Product(), new Product() };
        _mockProductRepo.Setup(repo =>
            repo.GetAllWithSpecAsync(It.IsAny<ProductSpecification>()))
            .ReturnsAsync(testProducts);

        // Act
        var result = await _productService.GetAllProductsAsync(10, 1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(testProducts);
        _mockImportData.Verify(m => m.ImportProductsFromExcel(), Times.Never);
    }

    [Fact]
    public async Task GetAllProductsAsync_NoProducts_ImportsAndReturnsProducts()
    {
        var testProducts = new List<Product> { new Product() };
        _mockProductRepo.SetupSequence(repo =>
            repo.GetAllWithSpecAsync(It.IsAny<ProductSpecification>()))
            .ReturnsAsync(new List<Product>())
            .ReturnsAsync(testProducts);

        //_mockImportData.Setup(m => m.ImportProductsFromExcel())
        //    .Returns(1);

        var result = await _productService.GetAllProductsAsync(10, 1);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(testProducts);
        _mockImportData.Verify(m => m.ImportProductsFromExcel(), Times.Once);
        _mockProductRepo.Verify(repo =>
            repo.GetAllWithSpecAsync(It.IsAny<ProductSpecification>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetAllProductsAsync_ImportDoesNotAddProducts_ReturnsEmpty()
    {
        // Arrange
        _mockProductRepo.SetupSequence(repo =>
            repo.GetAllWithSpecAsync(It.IsAny<ProductSpecification>()))
            .ReturnsAsync(new List<Product>())
            .ReturnsAsync(new List<Product>());

        //_mockImportData.Setup(m => m.ImportProductsFromExcel())
        //    .Returns(Task.CompletedTask);

        // Act
        var result = await _productService.GetAllProductsAsync(10, 1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
        _mockImportData.Verify(m => m.ImportProductsFromExcel(), Times.Once);
    }

    [Fact]
    public async Task GetAllProductsAsync_DatabaseErrorOnFirstGet_ReturnsFailure()
    {
        // Arrange
        var exceptionMessage = "Database connection failed";
        _mockProductRepo.Setup(repo =>
            repo.GetAllWithSpecAsync(It.IsAny<ProductSpecification>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _productService.GetAllProductsAsync(10, 1);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(500);
        result.Error.Title.Should().Be(exceptionMessage);
        _mockImportData.Verify(m => m.ImportProductsFromExcel(), Times.Never);
    }

    [Fact]
    public async Task GetAllProductsAsync_ImportThrowsError_ReturnsFailure()
    {
        var exceptionMessage = "File not found";
        _mockProductRepo.Setup(repo =>
            repo.GetAllWithSpecAsync(It.IsAny<ProductSpecification>()))
            .ReturnsAsync(new List<Product>());

        _mockImportData.Setup(m => m.ImportProductsFromExcel())
            .ThrowsAsync(new FileNotFoundException(exceptionMessage));
        var result = await _productService.GetAllProductsAsync(10, 1);
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(500);
        result.Error.Title.Should().Contain(exceptionMessage);
        _mockImportData.Verify(m => m.ImportProductsFromExcel(), Times.Once);
    }

    [Fact]
    public async Task GetAllProductsAsync_ErrorAfterImport_ReturnsFailure()
    {
        // Arrange
        var exceptionMessage = "Second query failed";
        _mockProductRepo.SetupSequence(repo =>
            repo.GetAllWithSpecAsync(It.IsAny<ProductSpecification>()))
            .ReturnsAsync(new List<Product>())
            .ThrowsAsync(new Exception(exceptionMessage));

        //_mockImportData.Setup(m => m.ImportProductsFromExcel())
        //    .Returns(Task.CompletedTask);

        var result = await _productService.GetAllProductsAsync(10, 1);
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(500);
        result.Error.Title.Should().Be(exceptionMessage);
        _mockImportData.Verify(m => m.ImportProductsFromExcel(), Times.Once);
    }
 

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task UpdateProductAsync_InvalidProductId_ReturnsFailure(int invalidId)
    {
        // Act
        var result = await _productService.UpdateProductAsync(invalidId, new Product());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(400);
        result.Error.StatusCode.Should().Be(400);
        _mockProductRepo.Verify(repo => repo.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProductAsync_NullProduct_ReturnsFailure()
    {
        var result = await _productService.UpdateProductAsync(1, null);
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(404);
        _mockProductRepo.Verify(repo => repo.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProductAsync_ProductNotFound_ReturnsFailure()
    {
        // Arrange
        const int productId = 1;
        _mockProductRepo.Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync((Product)null);

        // Act
        var result = await _productService.UpdateProductAsync(productId, new Product());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(404);
        _mockProductRepo.Verify(repo => repo.GetByIdAsync(productId), Times.Once);
    }

    [Fact]
    public async Task UpdateProductAsync_IdMismatch_ReturnsFailure()
    {
        // Arrange
        const int productId = 1;
        var existingProduct = new Product { Id = productId };
        var updatedProduct = new Product { Id = 2 };

        _mockProductRepo.Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);

        // Act
        var result = await _productService.UpdateProductAsync(productId, updatedProduct);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(400);
        _mockProductRepo.Verify(repo => repo.Update(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProductAsync_ValidRequest_UpdatesProduct()
    {
        const int productId = 1;
        var existingProduct = new Product { Id = productId };
        var updatedProduct = new Product { Id = productId };

        _mockProductRepo.Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);
        //_mockUnitOfWork.Setup(uow => uow.CompleteAsync())
        //    .Returns(Task.CompletedTask);

        var result = await _productService.UpdateProductAsync(productId, updatedProduct);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(updatedProduct);
        _mockProductRepo.Verify(repo => repo.Update(updatedProduct), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateProductAsync_DatabaseErrorOnGet_ReturnsFailure()
    {
        const int productId = 1;
        var exception = new Exception("Database failure");
        _mockProductRepo.Setup(repo => repo.GetByIdAsync(productId))
            .ThrowsAsync(exception);
        var result = await _productService.UpdateProductAsync(productId, new Product());
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(500);
        result.Error.Title.Should().Be(exception.Message);
    }

    [Fact]
    public async Task UpdateProductAsync_DatabaseErrorOnSave_ReturnsFailure()
    {
        const int productId = 1;
        var existingProduct = new Product { Id = productId };
        var updatedProduct = new Product { Id = productId };
        var exception = new Exception("Save failed");

        _mockProductRepo.Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);
        _mockUnitOfWork.Setup(uow => uow.CompleteAsync())
            .ThrowsAsync(exception);
        var result = await _productService.UpdateProductAsync(productId, updatedProduct);
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(500);
        result.Error.Title.Should().Be(exception.Message);
        _mockProductRepo.Verify(repo => repo.Update(updatedProduct), Times.Once);
    }


    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task DeleteProductAsync_InvalidId_ReturnsFailure(int invalidId)
    {
        // Act
        var result = await _productService.DeleteProductAsync(invalidId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Title.Should().Be("Invalid product ID");
        result.Error.StatusCode.Should().Be(400);

        _mockProductRepo.Verify(repo =>
            repo.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteProductAsync_ProductNotFound_ReturnsFailure()
    {
        const int productId = 1;
        _mockProductRepo.Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync((Product)null);

        var result = await _productService.DeleteProductAsync(productId);

        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(404);
        _mockProductRepo.Verify(repo =>
            repo.GetByIdAsync(productId), Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_SuccessfulDeletion_ReturnsSuccess()
    {
        // Arrange
        const int productId = 1;
        var product = new Product { Id = productId };
        _mockProductRepo.Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync(product);
        _mockUnitOfWork.Setup(uow => uow.CompleteAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _productService.DeleteProductAsync(productId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Product deleted successfully");
        _mockProductRepo.Verify(repo =>
            repo.Delete(product), Times.Once);
        _mockUnitOfWork.Verify(uow =>
            uow.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_NoChangesPersisted_ReturnsFailure()
    {
        // Arrange
        const int productId = 1;
        var product = new Product { Id = productId };
        _mockProductRepo.Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync(product);
        _mockUnitOfWork.Setup(uow => uow.CompleteAsync())
            .ReturnsAsync(0);

        // Act
        var result = await _productService.DeleteProductAsync(productId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Title.Should().Be("No changes persisted");
        _mockProductRepo.Verify(repo =>
            repo.Delete(product), Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_DatabaseErrorOnGet_ReturnsFailure()
    {
        // Arrange
        const int productId = 1;
        var exception = new Exception("Database connection failed");
        _mockProductRepo.Setup(repo => repo.GetByIdAsync(productId))
            .ThrowsAsync(exception);

        // Act
        var result = await _productService.DeleteProductAsync(productId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(500);
        result.Error.Title.Should().Be(exception.Message);
    }

    [Fact]
    public async Task DeleteProductAsync_ErrorDuringDeletion_ReturnsFailure()
    {
        // Arrange
        const int productId = 1;
        var product = new Product { Id = productId };
        var exception = new Exception("Deletion failed");
        _mockProductRepo.Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync(product);
        _mockProductRepo.Setup(repo => repo.Delete(It.IsAny<Product>()))
            .Throws(exception);

        // Act
        var result = await _productService.DeleteProductAsync(productId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(500);
        result.Error.Title.Should().Be(exception.Message);
        _mockProductRepo.Verify(repo =>
            repo.Delete(product), Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_ErrorDuringSave_ReturnsFailure()
    {
        // Arrange
        const int productId = 1;
        var product = new Product { Id = productId };
        var exception = new Exception("Save failed");
        _mockProductRepo.Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync(product);
        _mockUnitOfWork.Setup(uow => uow.CompleteAsync())
            .ThrowsAsync(exception);

        // Act
        var result = await _productService.DeleteProductAsync(productId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(500);
        result.Error.Title.Should().Be(exception.Message);
        _mockProductRepo.Verify(repo =>
            repo.Delete(product), Times.Once);
    }
 

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ArchiveProductAsync_InvalidId_ReturnsFailure(int invalidId)
    {
        // Act
        var result = await _productService.ArchiveProductAsync(invalidId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Title.Should().Be("Invalid product ID");
        result.Error.StatusCode.Should().Be(400);

        _mockProductRepo.Verify(repo =>
            repo.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ArchiveProductAsync_ProductNotFound_ReturnsFailure()
    {
        // Arrange
        const int productId = 1;
        _mockProductRepo.Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync((Product)null);

        // Act
        var result = await _productService.ArchiveProductAsync(productId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(404);
        _mockProductRepo.Verify(repo =>
            repo.GetByIdAsync(productId), Times.Once);
    }

    [Fact]
    public async Task ArchiveProductAsync_SuccessfulArchive_UpdatesProperties()
    {
        // Arrange
        const int productId = 1;
        var testProduct = new Product
        {
            Id = productId,
            IsDeleted = false,
            DeletedDate = DateTime.MinValue
        };

        _mockProductRepo.Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync(testProduct);
        //_mockUnitOfWork.Setup(uow => uow.CompleteAsync())
        //    .Returns(Task.CompletedTask);

        // Act
        var result = await _productService.ArchiveProductAsync(productId);
        var tolerance = TimeSpan.FromSeconds(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Product archived successfully");
        testProduct.IsDeleted.Should().BeTrue();
        testProduct.DeletedDate.Should()
            .BeCloseTo(DateTime.UtcNow, tolerance);
        _mockUnitOfWork.Verify(uow =>
            uow.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task ArchiveProductAsync_DatabaseErrorOnGet_ReturnsFailure()
    {
        // Arrange
        const int productId = 1;
        var exception = new Exception("Database connection failed");
        _mockProductRepo.Setup(repo => repo.GetByIdAsync(productId))
            .ThrowsAsync(exception);

        // Act
        var result = await _productService.ArchiveProductAsync(productId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(500);
        result.Error.Title.Should().Be(exception.Message);
    }

    [Fact]
    public async Task ArchiveProductAsync_DatabaseErrorOnSave_ReturnsFailure()
    {
        // Arrange
        const int productId = 1;
        var testProduct = new Product { Id = productId };
        var exception = new Exception("Save failed");

        _mockProductRepo.Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync(testProduct);
        _mockUnitOfWork.Setup(uow => uow.CompleteAsync())
            .ThrowsAsync(exception);

        // Act
        var result = await _productService.ArchiveProductAsync(productId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(500);
        result.Error.Title.Should().Be(exception.Message);
        testProduct.IsDeleted.Should().BeTrue();
        _mockUnitOfWork.Verify(uow =>
            uow.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task ArchiveProductAsync_AlreadyArchived_UpdatesDeletedDate()
    {
        // Arrange
        const int productId = 1;
        var originalDate = DateTime.UtcNow.AddDays(-1);
        var testProduct = new Product
        {
            Id = productId,
            IsDeleted = true,
            DeletedDate = originalDate
        };

        _mockProductRepo.Setup(repo => repo.GetByIdAsync(productId))
            .ReturnsAsync(testProduct);
        //_mockUnitOfWork.Setup(uow => uow.CompleteAsync())
        //    .Returns(Task.CompletedTask);

        // Act
        var result = await _productService.ArchiveProductAsync(productId);
        var tolerance = TimeSpan.FromSeconds(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        testProduct.DeletedDate.Should()
            .BeCloseTo(DateTime.UtcNow, tolerance);
        testProduct.IsDeleted.Should().BeTrue();
    }


    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task UnarchiveProductAsync_InvalidId_ReturnsFailure(int invalidId)
    {
        // Act
        var result = await _productService.UnarchiveProductAsync(invalidId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Title.Should().Be("Invalid product ID");
        result.Error.StatusCode.Should().Be(400);

        _mockProductRepo.Verify(repo =>
            repo.GetByIdIgnoreFiltersAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UnarchiveProductAsync_ProductNotFound_ReturnsFailure()
    {
        // Arrange
        const int productId = 1;
        _mockProductRepo.Setup(repo => repo.GetByIdIgnoreFiltersAsync(productId))
            .ReturnsAsync((Product)null);

        // Act
        var result = await _productService.UnarchiveProductAsync(productId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(404);
        _mockProductRepo.Verify(repo =>
            repo.GetByIdIgnoreFiltersAsync(productId), Times.Once);
    }

    [Fact]
    public async Task UnarchiveProductAsync_SuccessfulUnarchive_UpdatesProduct()
    {
        // Arrange
        const int productId = 1;
        var testProduct = new Product
        {
            Id = productId,
            IsDeleted = true,
            DeletedDate = DateTime.UtcNow
        };

        _mockProductRepo.Setup(repo => repo.GetByIdIgnoreFiltersAsync(productId))
            .ReturnsAsync(testProduct);
        //_mockUnitOfWork.Setup(uow => uow.CompleteAsync())
        //    .Returns(Task.CompletedTask);

        // Act
        var result = await _productService.UnarchiveProductAsync(productId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Product unarchived successfully");
        testProduct.IsDeleted.Should().BeFalse();
        _mockUnitOfWork.Verify(uow =>
            uow.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task UnarchiveProductAsync_AlreadyActive_StillReturnsSuccess()
    {
        // Arrange
        const int productId = 1;
        var testProduct = new Product
        {
            Id = productId,
            IsDeleted = false
        };

        _mockProductRepo.Setup(repo => repo.GetByIdIgnoreFiltersAsync(productId))
            .ReturnsAsync(testProduct);
        //_mockUnitOfWork.Setup(uow => uow.CompleteAsync())
        //    .Returns(Task.CompletedTask);

        // Act
        var result = await _productService.UnarchiveProductAsync(productId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        testProduct.IsDeleted.Should().BeFalse();
        _mockUnitOfWork.Verify(uow =>
            uow.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task UnarchiveProductAsync_DatabaseErrorOnRetrieval_ReturnsFailure()
    {
        // Arrange
        const int productId = 1;
        var exception = new Exception("Database connection failed");
        _mockProductRepo.Setup(repo => repo.GetByIdIgnoreFiltersAsync(productId))
            .ThrowsAsync(exception);

        // Act
        var result = await _productService.UnarchiveProductAsync(productId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(500);
        result.Error.Title.Should().Be(exception.Message);
    }

    [Fact]
    public async Task UnarchiveProductAsync_DatabaseErrorOnSave_ReturnsFailure()
    {
        // Arrange
        const int productId = 1;
        var testProduct = new Product { Id = productId };
        var exception = new Exception("Save failed");

        _mockProductRepo.Setup(repo => repo.GetByIdIgnoreFiltersAsync(productId))
            .ReturnsAsync(testProduct);
        _mockUnitOfWork.Setup(uow => uow.CompleteAsync())
            .ThrowsAsync(exception);

        // Act
        var result = await _productService.UnarchiveProductAsync(productId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(500);
        result.Error.Title.Should().Be(exception.Message);
        testProduct.IsDeleted.Should().BeFalse();
        _mockUnitOfWork.Verify(uow =>
            uow.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task UnarchiveProductAsync_UpdatesOnlyIsDeleted_LeavesOtherProperties()
    {
        // Arrange
        const int productId = 1;
        var originalDate = DateTime.UtcNow.AddDays(-1);
        var testProduct = new Product
        {
            Id = productId,
            Name = "Original Name",
            IsDeleted = true,
            DeletedDate = originalDate
        };

        _mockProductRepo.Setup(repo => repo.GetByIdIgnoreFiltersAsync(productId))
            .ReturnsAsync(testProduct);
        //_mockUnitOfWork.Setup(uow => uow.CompleteAsync())
        //    .Returns(Task.CompletedTask);

        // Act
        var result = await _productService.UnarchiveProductAsync(productId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        testProduct.IsDeleted.Should().BeFalse();
        testProduct.DeletedDate.Should().Be(originalDate);
        testProduct.Name.Should().Be("Original Name");
    }



    [Fact]
    public async Task SearchProductsAsync_NullParameters_ReturnsFailure()
    {
        // Act
        var result = await _productService.SearchProductsAsync(null);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Title.Should().Be("Search parameters cannot be null");
        result.Error.StatusCode.Should().Be(400);

        _mockProductRepo.Verify(repo =>
            repo.GetAllWithSpecAsync(It.IsAny<ProductSpecification>()), Times.Never);
    }

    //[Fact]
    //public async Task SearchProductsAsync_ValidParameters_ReturnsProducts()
    //{
    //    // Arrange
    //    var searchParams = new ProductSearchParameters("test");
    //    var expectedProducts = new List<Product> { new Product(), new Product() };

    //    _mockProductRepo.Setup(repo =>
    //        repo.GetAllWithSpecAsync(It.IsAny<ProductSpecification>()))
    //        .ReturnsAsync(expectedProducts);

    //    // Act
    //    var result = await _productService.SearchProductsAsync(searchParams);

    //    // Assert
    //    result.IsSuccess.Should().BeTrue();
    //    result.Value.Should().BeEquivalentTo(expectedProducts);
    //    _mockProductRepo.Verify(repo =>
    //        repo.GetAllWithSpecAsync(It.IsAny<ProductSpecification>()), Times.Once);
    //}

    //[Fact]
    //public async Task SearchProductsAsync_EmptyResults_ReturnsSuccess()
    //{
    //    // Arrange
    //    var searchParams = new ProductSearchParameters("nonexistent");
    //    _mockProductRepo.Setup(repo =>
    //        repo.GetAllWithSpecAsync(It.IsAny<ProductSpecification>()))
    //        .ReturnsAsync(new List<Product>());

    //    // Act
    //    var result = await _productService.SearchProductsAsync(searchParams);

    //    // Assert
    //    result.IsSuccess.Should().BeTrue();
    //    result.Value.Should().BeEmpty();
    //}

    //[Fact]
    //public async Task SearchProductsAsync_DatabaseError_ReturnsFailure()
    //{
    //    // Arrange
    //    var searchParams = new ProductSearchParameters("test");
    //    var exception = new Exception("Database connection failed");

    //    _mockProductRepo.Setup(repo =>
    //        repo.GetAllWithSpecAsync(It.IsAny<ProductSpecification>()))
    //        .ThrowsAsync(exception);

    //    // Act
    //    var result = await _productService.SearchProductsAsync(searchParams);

    //    // Assert
    //    result.IsSuccess.Should().BeFalse();
    //    result.Error.StatusCode.Should().Be(500);
    //    result.Error.Title.Should().Be(exception.Message);
    //}

    //[Fact]
    //public async Task SearchProductsAsync_ValidatesSpecificationUsage()
    //{
    //    // Arrange
    //    var searchParams = new ProductSearchParameters("test");
    //    var expectedSpec = new ProductSpecification(searchParams);
    //    ProductSpecification? actualSpec = null;

    //    _mockProductRepo.Setup(repo =>
    //        repo.GetAllWithSpecAsync(It.IsAny<ProductSpecification>()))
    //        .Callback<ProductSpecification>(s => actualSpec = s)
    //        .ReturnsAsync(new List<Product>());

    //    // Act
    //    await _productService.SearchProductsAsync(searchParams);

        
    //    actualSpec.Should().NotBeNull();
    //    //actualSpec!.SearchParameters.Should().BeEquivalentTo(searchParams);
    //}



    [Fact]
    public async Task FilterProductsAsync_NullFilter_ReturnsFailure()
    {
        // Act
        var result = await _productService.FilterProductsAsync(null);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Title.Should().Be("Filter parameters cannot be null");
        result.Error.StatusCode.Should().Be(400);

        _mockProductRepo.Verify(repo =>
            repo.GetAllWithSpecAsync(It.IsAny<ProductSpecification>()), Times.Never);
    }

    //[Fact]
    //public async Task FilterProductsAsync_ValidFilter_ReturnsProducts()
    //{
    //    // Arrange
    //    var filter = new ProductFilterParameters(minPrice: 10, maxPrice: 100);
    //    var expectedProducts = new List<Product> { new Product(), new Product() };

    //    _mockProductRepo.Setup(repo =>
    //        repo.GetAllWithSpecAsync(It.IsAny<ProductSpecification>()))
    //        .ReturnsAsync(expectedProducts);

    //    // Act
    //    var result = await _productService.FilterProductsAsync(filter);

    //    // Assert
    //    result.IsSuccess.Should().BeTrue();
    //    result.Value.Should().BeEquivalentTo(expectedProducts);
    //    _mockProductRepo.Verify(repo =>
    //        repo.GetAllWithSpecAsync(It.IsAny<ProductSpecification>()), Times.Once);
    //}

    //[Fact]
    //public async Task FilterProductsAsync_EmptyResults_ReturnsSuccess()
    //{
    //    // Arrange
    //    var filter = new ProductFilterParameters(inStockOnly: true);
    //    _mockProductRepo.Setup(repo =>
    //        repo.GetAllWithSpecAsync(It.IsAny<ProductSpecification>()))
    //        .ReturnsAsync(new List<Product>());

    //    // Act
    //    var result = await _productService.FilterProductsAsync(filter);

    //    // Assert
    //    result.IsSuccess.Should().BeTrue();
    //    result.Value.Should().BeEmpty();
    //}

    [Fact]
    public async Task FilterProductsAsync_DatabaseError_ReturnsFailure()
    {
        // Arrange
        var filter = new ProductFilterParameters();
        var exception = new Exception("Database connection failed");

        _mockProductRepo.Setup(repo =>
            repo.GetAllWithSpecAsync(It.IsAny<ProductSpecification>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _productService.FilterProductsAsync(filter);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(500);
        result.Error.Title.Should().Be(exception.Message);
    }

    //[Fact]
    //public async Task FilterProductsAsync_CorrectSpecificationUsed()
    //{
    //    // Arrange
    //    var filter = new ProductFilterParameters(categoryId: 5);
    //    var expectedSpec = new ProductSpecification(filter);
    //    ProductSpecification? actualSpec = null;

    //    _mockProductRepo.Setup(repo =>
    //        repo.GetAllWithSpecAsync(It.IsAny<ProductSpecification>()))
    //        .Callback<ProductSpecification>(s => actualSpec = s)
    //        .ReturnsAsync(new List<Product>());

    //    // Act
    //    await _productService.FilterProductsAsync(filter);

    //    // Assert
    //    actualSpec.Should().NotBeNull();
    //    actualSpec!.FilterParameters.Should().BeEquivalentTo(filter);
    //}

    //[Fact]
    //public async Task FilterProductsAsync_ComplexFilter_PassesCorrectParameters()
    //{
    //    // Arrange
    //    var complexFilter = new ProductFilterParameters(
    //        minPrice: 50,
    //        maxPrice: 200,
    //        categoryId: 3,
    //        inStockOnly: true
    //    );

    //    ProductFilterParameters? capturedParams = null;

    //    _mockProductRepo.Setup(repo =>
    //        repo.GetAllWithSpecAsync(It.IsAny<ProductSpecification>()))
    //        .Callback<ProductSpecification>(s => capturedParams = s.FilterParameters)
    //        .ReturnsAsync(new List<Product>());

    //    // Act
    //    await _productService.FilterProductsAsync(complexFilter);

    //    // Assert
    //    capturedParams.Should().NotBeNull();
    //    capturedParams.Should().BeEquivalentTo(complexFilter);
    //}
 

    [Fact]
    public async Task SortProductsAsync_NullParameters_ReturnsFailure()
    {
        // Act
        var result = await _productService.SortProductsAsync(null);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Title.Should().Be("Sort parameters cannot be null");
        result.Error.StatusCode.Should().Be(400);

        _mockProductRepo.Verify(repo =>
            repo.GetAllWithSpecAsync(It.IsAny<ProductSpecification>()), Times.Never);
    }

    //[Fact]
    //public async Task SortProductsAsync_ValidSort_ReturnsOrderedProducts()
    //{
    //    // Arrange
    //    var sortParams = new ProductFilterParameters(sortBy: "Price", sortDirection: "asc");
    //    var products = new List<Product>
    //    {
    //        new Product { Cost = 100 },
    //        new Product { Cost = 50 }
    //    };

    //    _mockProductRepo.Setup(repo =>
    //        repo.GetAllWithSpecAsync(It.IsAny<ProductSpecification>()))
    //        .ReturnsAsync(products.OrderBy(p => p.Cost).ToList());

    //    // Act
    //    var result = await _productService.SortProductsAsync(sortParams);

    //    // Assert
    //    result.IsSuccess.Should().BeTrue();
    //    result.Value.Should().BeInAscendingOrder(p => p.Cost);
    //    _mockProductRepo.Verify(repo =>
    //        repo.GetAllWithSpecAsync(It.IsAny<ProductSpecification>()), Times.Once);
    //}

    //[Fact]
    //public async Task SortProductsAsync_EmptyResults_ReturnsSuccess()
    //{
    //    // Arrange
    //    var sortParams = new ProductFilterParameters(sortBy: "Name");
    //    _mockProductRepo.Setup(repo =>
    //        repo.GetAllWithSpecAsync(It.IsAny<ProductSpecification>()))
    //        .ReturnsAsync(new List<Product>());

    //    // Act
    //    var result = await _productService.SortProductsAsync(sortParams);

    //    // Assert
    //    result.IsSuccess.Should().BeTrue();
    //    result.Value.Should().BeEmpty();
    //}

    [Fact]
    public async Task SortProductsAsync_DatabaseError_ReturnsFailure()
    {
        // Arrange
        var sortParams = new ProductFilterParameters();
        var exception = new Exception("Database timeout");

        _mockProductRepo.Setup(repo =>
            repo.GetAllWithSpecAsync(It.IsAny<ProductSpecification>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _productService.SortProductsAsync(sortParams);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(500);
        result.Error.Title.Should().Be(exception.Message);
    }

    //[Fact]
    //public async Task SortProductsAsync_CorrectSortParametersUsed()
    //{
    //    // Arrange
    //    var sortParams = new ProductFilterParameters(
    //        sortBy: "DateCreated",
    //        sortDirection: "desc"
    //    );

    //    ProductFilterParameters? capturedParams = null;

    //    _mockProductRepo.Setup(repo =>
    //        repo.GetAllWithSpecAsync(It.IsAny<ProductSpecification>()))
    //        .Callback<ProductSpecification>(s => capturedParams = s.FilterParameters)
    //        .ReturnsAsync(new List<Product>());

    //    // Act
    //    await _productService.SortProductsAsync(sortParams);

    //    // Assert
    //    capturedParams.Should().NotBeNull();
    //    capturedParams!.SortBy.Should().Be("DateCreated");
    //    capturedParams.SortDirection.Should().Be("desc");
    //}

    //[Fact]
    //public async Task SortProductsAsync_MultipleSortFields_PassesAllParameters()
    //{
    //    // Arrange
    //    var sortParams = new ProductFilterParameters(
    //        sortBy: "Category,Price",
    //        sortDirection: "asc,desc"
    //    );

    //    ProductFilterParameters? capturedParams = null;

    //    _mockProductRepo.Setup(repo =>
    //        repo.GetAllWithSpecAsync(It.IsAny<ProductSpecification>()))
    //        .Callback<ProductSpecification>(s => capturedParams = s.FilterParameters)
    //        .ReturnsAsync(new List<Product>());

    //    // Act
    //    await _productService.SortProductsAsync(sortParams);

    //    // Assert
    //    capturedParams.Should().NotBeNull();
    //    capturedParams!.SortBy.Should().Be("Category,Price");
    //    capturedParams.SortDirection.Should().Be("asc,desc");
    //}

}




