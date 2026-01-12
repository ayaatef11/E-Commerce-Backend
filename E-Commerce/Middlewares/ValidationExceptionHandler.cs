namespace E_Commerce.Middlewares;

public sealed class ValidationExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
            return false;

        var errors = validationException.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        var problemDetails = new ProblemDetails
        {
            Title = "Validation Failed",
            Detail = "One or more validation error",
            Status = StatusCodes.Status400BadRequest
        };

        problemDetails.Extensions.Add("errors", errors);

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails,
        };

        return await problemDetailsService.TryWriteAsync(context);
    }
}
