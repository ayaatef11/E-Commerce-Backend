namespace E_Commerce.Infrastructure.Persistence.Seeding.ExcelParser;
public interface IImportProductData
{
    Task<IEnumerable<Product>> ImportProductsFromExcel();
}

