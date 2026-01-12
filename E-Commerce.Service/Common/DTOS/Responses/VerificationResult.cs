namespace E_Commerce.Application.Common.DTOS.Responses;

public class VerificationResult
{
    public bool Succeeded { get; }
    public string Error { get; }

    private VerificationResult(bool success, string error)
    {
        Succeeded = success;
        Error = error;
    }

    public static VerificationResult Success() => new(true, null);
    public static VerificationResult Failure(string error) => new(false, error);
}
