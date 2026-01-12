using E_Commerce.Application.Interfaces.Common;
using E_Commerce.Repository.Repositories.Interfaces;

namespace Services.Common
{
    public class CartServiceWithRedis(UserManager<AppUser>_userManager,IUnitOfWork _unitOfWork,ICartRepositoryWithRedis _cartRepository,IHttpContextAccessor _httpContextAccessor) : ICartServiceWithRedis

    {
       /* string? _userId = _httpContextAccessor.HttpContext?.User?
 .FindFirst(ClaimTypes.NameIdentifier)?.Value;
        public async  Task AddItemToCartAsync(string userId, int productId, int quantity)
        {
           var user=await _userManager.FindByIdAsync(userId);
            if (user == null) return ;
            var product=await _unitOfWork.Repository<Product>().GetByIdAsync(productId);
            if (product == null||product.StockQueantity<quantity) return ;
            
            var cartItem = new CartItem()
            {
                ProductId = productId,
                Quantity = quantity,
                Name = product.Name,
                PictureUrl = product.PictureUrl,
                Price = product.Price * quantity
            };
            product.StockQueantity-=quantity;
            
        }

   

       *//* public async Task ClearCartAsync(string userId)
        {
         var user=await _userManager.FindByIdAsync(userId);
            if (user == null) return;
            var cart = user.Cart;
            if (cart == null) return;
            cart.Items.Clear();

            await _cartRepository.UpdateCartAsync(cart);
        }*/


       /* public async Task<Cart?> UpdateCartItemQuantityAsync(string userId, int productId, int newQuantity)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;
            var cart = user.Cart;
            if (cart == null) return null;
            var cartItem = await _cartRepository.GetItemFromCartAsync(cart, i => i.ProductId == productId);
            if (cartItem == null) return null;
            cartItem.Quantity = newQuantity;
            await _cartRepository.UpdateCartAsync(cart);
            return cart;
        }*//*
        public async Task<Cart?> GetUserCartAsync()
        {
            if(_userId==null)return null;
            return await GetUserCartAsync(_userId);
        }
        public async  Task<Cart?> AddItemToCartAsync(int productId, int quantity)
        {
            if (_userId == null) return null;

            return await  AddItemToCartAsync(_userId, productId, quantity);
        }
       *//* public async Task<Cart?> UpdateCartItemQuantityAsync(int productId, int newQuantity)
        {
            if (_userId == null) return null;
           return await UpdateCartItemQuantityAsync(_userId, productId, newQuantity);
        }*//*
        public async Task<Cart?> RemoveItemFromCartAsync(int productId)
        {
            if (_userId == null) return null;
            return await RemoveItemFromCartAsync(_userId,productId);
        }
       *//* public async Task ClearCartAsync()
        {
            if (_userId == null) return ;
             await ClearCartAsync(_userId);
        }*//*
        public async Task<decimal> CalculateCartTotalAsync()
        {
            if (_userId == null) return 0;
          return await   CalculateCartTotalAsync(_userId);
        }
*/
    }
}
