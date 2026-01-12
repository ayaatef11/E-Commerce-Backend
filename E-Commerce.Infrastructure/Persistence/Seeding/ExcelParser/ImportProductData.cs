using E_Commerce.Core.Shared.Utilties;
using OfficeOpenXml;
namespace E_Commerce.Infrastructure.Persistence.Seeding.ExcelParser;
public class ImportProductData(StoreContext _context):IImportProductData
{
    private const string FilePath = PATH.ExcelFilePath;

    public async Task<IEnumerable<Product>> ImportProductsFromExcel()
    {
        if (!File.Exists(FilePath))
        {
            throw new FileNotFoundException("Excel file not found", FilePath);
        }

        var products = new List<Product>();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using (var stream = new FileStream(FilePath, FileMode.Open))
        {
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension?.Rows ?? 0;

                for (int row = 2; row <= rowCount; row++) 
                {
                    var product = ParseProductRow(worksheet, row);
                    if (product != null)
                    {
                        products.Add(product);
                    }
                }
            }
        }

        var validProducts = products.Where(p =>
            !string.IsNullOrEmpty(p.Name) &&
            p.Cost > 0 &&
            p.StockQuantity >= 0).ToList();

        if (validProducts.Count == 0)
        {
            return Enumerable.Empty<Product>();
        }
        var executionStrategy = _context.Database.CreateExecutionStrategy();
        IEnumerable<Product> result = Enumerable.Empty<Product>();
        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Products.AddRangeAsync(validProducts);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                result = validProducts;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new ApplicationException("Error saving products to database", ex);
            }
        });
        return result;
    }
    private Product? ParseProductRow(ExcelWorksheet worksheet, int row)
    {
        try
        {
            return new Product
            {
                Mandop = ParseDecimal(worksheet.Cells[row, 2]),
                GomlaPrice = ParseDecimal(worksheet.Cells[row, 3]),
                ListPrice = ParseDecimal(worksheet.Cells[row, 5]),
                Gomla = ParseDecimal(worksheet.Cells[row,8]),
                Cost = ParseDecimal(worksheet.Cells[row, 16]),
                StockQuantity = ParseInt(worksheet.Cells[row, 17]),
                Name = worksheet.Cells[row, 18].Value?.ToString().Trim() ?? string.Empty,
                Description = "Default Description", 
                PictureUrl = "Default Url", 
                IsDeleted = false,
                DeletedDate = DateTime.MinValue
            };
        }
        catch
        {
            return null;
        }
    }

    private decimal ParseDecimal(ExcelRange cell)
    {
        return decimal.TryParse(cell.Value?.ToString().Trim(), out decimal result)
            ? result
            : 0m;
    }

    private int ParseInt(ExcelRange cell)
    {
        return int.TryParse(cell.Value?.ToString().Trim(), out int result)
            ? result
            : 0;
    }
}