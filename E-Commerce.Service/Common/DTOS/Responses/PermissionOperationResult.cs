namespace E_Commerce.Application.Common.DTOS.Responses;
public class PermissionOperationResult
{
    public bool Success { get; }
    public string Message { get; }

    public PermissionOperationResult(bool success, string message)
    {
        Success = success;
        Message = message;
    }
}

