namespace E_Commerce.Core.Shared.Results;
public class Error
{
    public static readonly Error? None = new("OK", string.Empty,200);

    public int StatusCode { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public Error()
    {
        Title = string.Empty;
        StatusCode = 400;
    }
    public Error(string message, string title  , int statusCode)
    {
        StatusCode = statusCode;
        Title = title ?? GetDefaultMessageForStatusCode(statusCode);
        Message = message;
    }

    public static readonly Error Unauthorized = new("Auth.Unauthorized", "Authentication required", 401);
    public static readonly Error UserNotFound = new("User.NotFound", "User not found", 404);
    public static readonly Error EmptyCart = new("Cart.Empty", "Cart contains no items", 400);

    public static Error ProductNotFound(int productId) => new(
       "Product.NotFound",
       $"Product with ID {productId} not found",
       404
   );

    public static Error InsufficientStock(int productId, int available, int requested) => new(
       "Stock.Insufficient",
       $"Product {productId}: Only {available} available (requested {requested})",
       400
   );

    public static Error NegativeStock(int productId) => new(
        "Stock.Negative",
        $"Deduction would result in negative stock for product {productId}",
        400
    );

    public static Error DatabaseError(string message) => new(
        "Database.Error",
        message,
        500
    );

    private string GetDefaultMessageForStatusCode(int statusCode)
    {
        return statusCode switch
        {
            400 => "A bad request, you have made!",
            401 => "Authorized, you are not!",
            404 => "Resource was not found!",
            500 => "Server Error",
            _ => "Invalid request"
        };
    }
}
