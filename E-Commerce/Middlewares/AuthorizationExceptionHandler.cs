namespace E_Commerce.Middlewares;


public class AuthorizationExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not UnauthorizedAccessException validationException)
            return false;

        var problemDetails = new ProblemDetails
        {
            Title = "Forbidden",
            Detail = "You do not have permission to perform this action.",
            Status = StatusCodes.Status403Forbidden
        };

        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;

        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails,
        };

        return await problemDetailsService.TryWriteAsync(context);
    }
}

