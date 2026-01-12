using E_Commerce.Core.Shared.Utilties;

namespace E_Commerce.Infrastructure.Persistence.Seeding.JsonParser;
public class StoreContextSeed
{
    public async static Task SeedProductDataAsync(StoreContext _storeContext)
    {

        if (!_storeContext.Products.Any())
        {
        var ProductsJSONData = File.ReadAllText(PATH.ProductJson);

        var products = JsonSerializer.Deserialize<List<Product>>(ProductsJSONData);

        if (products?.Count > 0)
        {
            foreach (var product in products)
            {
                _storeContext.Products.Add(product);
            }
        }
        }

        await _storeContext.SaveChangesAsync();
    }

}

