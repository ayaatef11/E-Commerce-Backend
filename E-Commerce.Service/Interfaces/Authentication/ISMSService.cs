using Twilio.Rest.Api.V2010.Account;

namespace E_Commerce.Application.Interfaces.Authentication
{
    public interface ISMSService
    {
        MessageResource Send(string mobileNumber, string body);

    }
}
