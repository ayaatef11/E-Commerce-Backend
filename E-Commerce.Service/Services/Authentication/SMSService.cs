using E_Commerce.Application.Interfaces.Authentication;
using E_Commerce.Core.Shared.Settings;
using Microsoft.Extensions.Options;
using Twilio.Rest.Api.V2010.Account;

namespace E_Commerce.Application.Services.Authentication;
    public class SMSService(IOptions<TwilioSettings> twilio) : ISMSService
    {
        private readonly TwilioSettings _twilio = twilio.Value;

        public MessageResource Send(string mobileNumber, string body)
        {
            Twilio.TwilioClient.Init(_twilio.AccountSID, _twilio.AuthToken);

            var result = MessageResource.Create(
                    body: body,
                    from: new Twilio.Types.PhoneNumber(_twilio.TwilioPhoneNumber),
                    to: mobileNumber
                );

            return result;
        }
    }


