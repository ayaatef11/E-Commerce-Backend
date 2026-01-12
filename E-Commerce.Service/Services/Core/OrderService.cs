using E_Commerce.Core.Data;
using E_Commerce.Repository.Specifications.OrderSpecifications;
using E_Commerce.Application.Interfaces.Authentication;
using E_Commerce.Application.Interfaces.Core;
using E_Commerce.Core.Shared.Results;
using E_Commerce.Repository.Repositories.Interfaces;
namespace E_Commerce.Application.Services.Core;
    public class OrderService(IUnitOfWork _unitOfWork,UserManager<AppUser>_userManager,StoreContext _context,IEmailSenderService _emailSenderService) : IOrderService
    {

        #region Private functions
        public Result<Order> ValidateOrder(Order order)
        {
            if (order.Items.Count == 0)
            {
                return Result.Failure<Order>(new Error
                {
                    Title = "Empty Order",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }
            return Result.Success(order);
        }
        private async Task<Result<Order>> SetBuyerInfoAsync(Order order)
        {
            var user = await _userManager.FindByEmailAsync(order.BuyerEmail);
            if (user == null)
            {
                return Result.Failure<Order>(new Error
                {
                    Title = "User Not Found",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }
            order.BuyerName = user.Full_Name;
            order.BuyerPhoneNumber = user.PhoneNumber ?? "_";
            order.BuyerAddress = user.Address??"_";
            return Result.Success(order);
        }

        private async Task ProcessItemsAsync(Order order)
        {
            foreach (var item in order.Items.ToList())  
            {
                var existingProduct = await _unitOfWork.Repository<Product>().GetByNameAsync(item.Product.Name);
                if (existingProduct == null)
                {
                //how to add a message for user that product here isnt found
                    order.Items.Remove(item); 
                    continue;
                }
            item.Product = existingProduct;
            item.Price = existingProduct.Price;
            if (item.Quantity > existingProduct.StockQuantity)
                {
                    item.Quantity = existingProduct.StockQuantity; 
                //add a message for user that the amount is insufficient so we made it equal to the one in the database
                    if (item.Quantity == 0)
                    {
                        order.Items.Remove(item); 
                        continue;
                    }
                }
                
                existingProduct.StockQuantity -= item.Quantity;
            _unitOfWork.Repository<Product>().Update(existingProduct);

            
        }
        order.Price = order.Items.Sum(item => item.Total);

    }
    private async Task SaveOrderAsync(Order order)
    {
        try
        { 
            order.OrderDate = DateTimeOffset.UtcNow;
             _unitOfWork.Repository<Order>().Update(order);
            await _unitOfWork.Repository<Order>().AddAsync(order); // Add فقط


            var result = await _unitOfWork.CompleteAsync();
            Console.WriteLine($"CompleteAsync result: {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in SaveOrderAsync: {ex}");
            throw;
        }
    }
      

        #endregion

        public async Task<Result<Order>> CreateOrder(Order order)
        {
         
            var validationResult = ValidateOrder(order);
            if (validationResult.IsFailure)
                return validationResult;

            var buyerResult = await SetBuyerInfoAsync(order);
            if (buyerResult.IsFailure)
                return buyerResult;

            await ProcessItemsAsync(order);

            if (order.Items.Count == 0)  
            {
                return Result.Failure<Order>(new Error
                {
                    Title = "No Valid Items",
                    StatusCode = StatusCodes.Status404NotFound
                });
            }

            await SaveOrderAsync(order);
            return Result.Success(order);
        }

    public async Task<Result<Order>> GetById(int id, string BuyerEmail)
    {
        if (id <= 0)
        {
            return Result.Failure<Order>(
                new Error("ID must be a positive integer", "Invalid ID", 400)
            );
        }
        bool isEmailValid = _emailSenderService.ValidateEmail(BuyerEmail);
        if (!isEmailValid)
        {
            return Result.Failure<Order>(
                new Error("Invalid email address format", "Invalid Email", 400)
            );
        }
        var spec = new OrderSpecifications(BuyerEmail, id);
        var order = await _context.Orders
     .Include(o => o.Items)
     .ThenInclude(i => i.Product)
     .FirstOrDefaultAsync(o => o.Id == id); 
        if (order == null)
        {
            return Result.Failure<Order>(
                new Error($"Order {id} not found for {BuyerEmail}", "Order Not Found", 404)
            );
        }

        return Result.Success(order);
    }
   
    public async Task<Result<Order>> GetById(int id)
    {
        var order = await _context.Orders
     .Include(o => o.Items)
     .ThenInclude(i => i.Product)
     .FirstOrDefaultAsync(o => o.Id == id);
        if (order == null)
        {
            return Result.Failure<Order>(
                new Error($"Order {id} not found ", "Order Not Found", 404)
            );
        }

        return Result.Success(order);
    }
  
    public async Task<Result<Order>> AddProductToOrder(int orderId, OrderItem item)
        {
        if (orderId <= 0)
        {
            return Result.Failure<Order>(new Error
            {
                Title = "order id isn't valid ",
                StatusCode = StatusCodes.Status400BadRequest,
            });
        }
        if (_context.Orders == null)
        {
            return Result.Failure<Order>(new Error
            {
                Title = "No orders found in the system",
                StatusCode = StatusCodes.Status404NotFound,
            });
        }
            var order = await _context.Orders
    .Include(o => o.Items)
    .ThenInclude(i => i.Product)
    .FirstOrDefaultAsync(o => o.Id == orderId);   

            if (order == null)
            {
                order = new Order
                {
                    Status = OrderStatus.Pending,
                    Items = new List<OrderItem>()
                };
                await _unitOfWork.Repository<Order>().AddAsync(order);
            }

            if (order.Status == OrderStatus.Canceled)
            {
                return Result.Failure<Order>(new Error
                {
                    Title = "Cant Modify canceled order..",
                    StatusCode = StatusCodes.Status400BadRequest,
                });
            }

            var product = await _unitOfWork.Repository<Product>().GetByNameAsync(item.Product.Name);

            if (product == null)
                return Result.Failure<Order>(new Error
                {
                    Title = "Product not found ",
                    StatusCode = StatusCodes.Status404NotFound,
                });

            if (product.StockQuantity < item.Quantity)
                return Result.Failure<Order>(new Error
                {
                    Title = "Insufficient Quantity ",
                    StatusCode = StatusCodes.Status400BadRequest,
                });

            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                Product = product,
                Quantity = item.Quantity,
                Price = product.Price  
            };

            product.StockQuantity -= item.Quantity;
        //-----------------
        //order.TotalPrice += item.Total;
        decimal pf = order.Items?.Sum(item => item.Total) ?? 0m;//order.TotalPrice;
        order.Price = pf;
            order.Items.Add(orderItem);       
        _unitOfWork.Repository<Order>().Update(order);
        await _unitOfWork.CompleteAsync();

            return Result.Success(order);
        }
       
    public async Task<Result<Order>> UpdateProductInOrder(int orderId, int itemId, int newQuantity)
        {
            if (newQuantity <= 0)
                return Result.Failure<Order>(new Error
                {
                    Title = "quantity must be greater than 0 ",
                    StatusCode = StatusCodes.Status400BadRequest,
                });
        if (orderId <= 0)
        {
            return Result.Failure<Order>(new Error
            {
                Title = "invalid order id ",
                StatusCode = StatusCodes.Status400BadRequest,
            });
        }
        if (itemId <= 0)
        {
            return Result.Failure<Order>(new Error
            {
                Title = "invalid item id ",
                StatusCode = StatusCodes.Status400BadRequest,
            });
        }
        var order = await _context.Orders
    .Include(o => o.Items)
        .ThenInclude(i => i.Product) 
    .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return Result.Failure<Order>(new Error
                {
                    Title = "Order Not found ",
                    StatusCode = StatusCodes.Status400BadRequest,
                });

            if (order.Status == OrderStatus.Canceled)
            {
                return Result.Failure<Order>(new Error
                {
                    Title = "order is canceled, you can't update an item to it  ",
                    StatusCode = StatusCodes.Status400BadRequest,
                });
            }
            var item = order.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
                return Result.Failure<Order>(new Error
                {
                    Title = "product Not found in order",
                    StatusCode = StatusCodes.Status400BadRequest,
                });
            var oldQuantity = item.Quantity;
        var oldPrice = item.Total;
            item.Quantity = newQuantity;
            item.Product.StockQuantity -= newQuantity - oldQuantity;
        //--
        //order.TotalPrice -= (item.Total - oldPrice);
        decimal pf = order.Items?.Sum(item => item.Total) ?? 0m;//order.TotalPrice;
        order.Price = pf;
        _unitOfWork.Repository<Product>().Update(item.Product);
            _unitOfWork.Repository<Order>().Update(order);
            await _unitOfWork.CompleteAsync();

            return Result.Success(order);
        }

    public async Task<Result<Order>> RemoveProductFromOrder(int orderId, int orderItemId)
        {
        if (orderId <= 0)
        {
            return Result.Failure<Order>(new Error
            {
                Title = "order id not valid ",
                StatusCode = StatusCodes.Status404NotFound,
            });
        }
        
            if (  orderItemId <= 0)
            {
                return Result.Failure<Order>(new Error
                {
                    Title = "order item id not valid ",
                    StatusCode = StatusCodes.Status404NotFound,
                });
            }
            
            var spec = new OrderSpecifications(orderId);
            var order = await _context.Orders
    .Include(o => o.Items)
        .ThenInclude(i => i.Product)
    .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return Result.Failure<Order>(new Error
                {
                    Title = "Order Not found ",
                    StatusCode = StatusCodes.Status404NotFound,
                });

            var item = order.Items.FirstOrDefault(i => i.Id == orderItemId);

            if (item == null)
                return Result.Failure<Order>(new Error
                {
                    Title = "Item not found in the order ",
                    StatusCode = StatusCodes.Status404NotFound,
                });

            item.Product.StockQuantity += item.Quantity;
            _unitOfWork.Repository<Product>().Update(item.Product);
        //-----------
        //order.TotalPrice -= item.Total;
        decimal pf = order.Items?.Sum(item => item.Total) ?? 0m;//order.TotalPrice;
        order.Price = pf;
        order.Items.Remove(item);
            _unitOfWork.Repository<OrderItem>().Delete(item);
            await _unitOfWork.CompleteAsync();
            return Result.Success(order);
        }

    public async Task<string> TrackOrderStatus(int orderId)
        {
        if (orderId <= 0) return "order id not valid";
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId);
            if (order == null) return "Order not found";
            return order.Status.ToString();
        }

    public async Task<Result<Order>> UpdateOrderStatus(int orderId, string newStatus)
    {
        if (orderId <= 0) {
            return Result.Failure<Order>(new Error
            {
                Title = "invalid id ",
                StatusCode = StatusCodes.Status404NotFound,
            });
        }
        var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId);
        if (order == null)
            return Result.Failure<Order>(new Error
            {
                Title = "order not found  ",
                StatusCode = StatusCodes.Status404NotFound,
            });
        if (!Enum.TryParse<OrderStatus>(newStatus, ignoreCase: true, out var status))
            return Result.Failure<Order>(new Error
            {
                Title = "invalid status  ",
                StatusCode = StatusCodes.Status400BadRequest,
            });
        if (order.Status.ToString() == newStatus)
        {
            return Result.Failure<Order>(new Error
            {
                Title = "This is the actual product status",
                StatusCode = StatusCodes.Status400BadRequest,
            });
        }
        order.Status = status;
        _unitOfWork.Repository<Order>().Update(order);
        await _unitOfWork.CompleteAsync();
        return Result.Success(order);
    }

    public async Task<Result<Order>> CancelOrder(int orderId, string email)
    {
        if (orderId <= 0)
        {
            return Result.Failure<Order>(new Error
            {
                Title = "invalid order id",
                StatusCode = StatusCodes.Status404NotFound,
            });
        }
        var result = _emailSenderService.ValidateEmail(email);
        if (!result)
        {
            return Result.Failure<Order>(new Error
            {
                Title = "Email not Valid",
                StatusCode = StatusCodes.Status404NotFound,
            });
        }

        var spec = new OrderSpecifications(email, orderId);

        var order = await _context.Orders
           .Include(o => o.Items)
           .ThenInclude(i => i.Product)
           .FirstOrDefaultAsync(o => o.Id == orderId); 
        if (order == null)
        {
            return Result.Failure<Order>(new Error
            {
                Title = "order not found ",
                StatusCode = StatusCodes.Status404NotFound,
            });
        }
        if (order.Status == OrderStatus.Canceled)
        {
            return Result.Failure<Order>(new Error
            {
                Title = "Cant cancel a cancelled order",
                StatusCode = StatusCodes.Status400BadRequest,
            });
        }
        foreach (var item in order.Items)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.ProductId);
            if (product == null) continue;//handle missing product
            product.StockQuantity += item.Quantity;
            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.CompleteAsync();
        }
        order.Status = OrderStatus.Canceled;
        order.Price = order.Price;
        _unitOfWork.Repository<Order>().Update(order);
        await _unitOfWork.CompleteAsync();
        return Result.Success(order);
    }

    public async Task<Result<List<Order>>> GetOrdersSortedByDateAsync(int page, int pageSize = 5, bool descending = true, string? buyerEmail = null)
    {
        try
        {
            var query = _context.Orders
                .Include(o => o.Items).ThenInclude(o => o.Product)
                .AsQueryable();

            // Filter by buyer email if provided (for regular users)
            if (!string.IsNullOrEmpty(buyerEmail))
            {
                query = query.Where(o => o.BuyerEmail == buyerEmail);
            }

            // Apply sorting
            query = descending
                ? query.OrderByDescending(o => o.OrderDate)
                : query.OrderBy(o => o.OrderDate);

            var orders =   await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return Result.Success(orders);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<Order>>(new Error(
                "Database.Error",
                ex.Message,
                StatusCodes.Status500InternalServerError));
        }
    }
}


