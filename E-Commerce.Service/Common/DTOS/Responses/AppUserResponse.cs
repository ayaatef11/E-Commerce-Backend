namespace E_Commerce.Application.Common.DTOS.Responses;
public sealed record AppUserResponse
{
    public string DisplayName { get; set; }
    public string Value { get; set; }
    public string AccessToken { get; set; }
    public string ExpireAt { get; set; }

    public AppUserResponse(string displayName, string value, string accessToken, string expireAt)
    {
        DisplayName = displayName;
        Value = value;
        AccessToken = accessToken;
        ExpireAt = expireAt;
    }
}

