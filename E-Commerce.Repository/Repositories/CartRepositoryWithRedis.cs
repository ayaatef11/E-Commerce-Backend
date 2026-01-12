using E_Commerce.Repository.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;

namespace E_Commerce.Repository.Repositories;
public class CartRepositoryWithRedis : ICartRepositoryWithRedis
{
    private readonly IDatabase _database;
    private readonly JsonSerializerOptions _options;
    private readonly StoreContext _context;
    IUnitOfWork _unitOfWork;
    public CartRepositoryWithRedis(IConnectionMultiplexer redis, IUnitOfWork unitofwork)
    {
        _database = redis.GetDatabase();
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        _unitOfWork = unitofwork;
    }

    public async Task<Cart?> GetCartAsync(string cartId)
    {
        var data = await _database.StringGetAsync(cartId);
        //if(data==null)return _unitOfWork.Repository<Cart>() 
        return data.IsNullOrEmpty ? null : JsonSerializer.Deserialize<Cart>(data!, _options);
    }
    public async Task<CartItem?> GetItemFromCartAsync(Cart cart, Func<CartItem, bool> predicate)
    {
        return cart.Items.FirstOrDefault(predicate);
    }

    public async Task<Cart> GetUserCartAsync(string userId)
    {
        return await _context.Carts
        .Include(c => c.Items)
        .FirstOrDefaultAsync(c => c.UserId == userId);
    }
    public async Task<bool> DeleteCartAsync(string cartId)
    {
        return await _database.KeyDeleteAsync(cartId);
    }


    /* public async Task<Cart?> UpdateCartAsync(Cart cart)
{
    var serializedCart = JsonSerializer.Serialize(cart, _options);
    var created = await _database.StringSetAsync(cart.Id, serializedCart, TimeSpan.FromDays(7));
    if (!created) return null;
    return await GetCartAsync(cart.Id);
}*/



    /*   public async Task<Cart?> AddItemToCartAsync(string cartId, CartItem newItem)
       {
           var cart = await GetCartAsync(cartId) ?? new Cart(cartId);

           var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == newItem.ProductId);

           if (existingItem != null)
           {
               existingItem.Quantity += newItem.Quantity;
           }
           else
           {
               cart.Items.Add(newItem);
           }

           return await UpdateCartAsync(cart);
       }*/


    /* public async Task<Cart?> RemoveItemFromCartAsync(string cartId, int productId)
     {
         var cart = await GetCartAsync(cartId);
         if (cart == null) return null;

         var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
         if (item == null) return cart;

         cart.Items.Remove(item);

         return await UpdateCartAsync(cart);
     }*/
}

