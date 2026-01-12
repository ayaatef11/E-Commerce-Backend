using E_Commerce.DTOS.Auth.Requests;
using E_Commerce.Application.Common.DTOS.Requests;

namespace E_Commerce.Mapping.AuthMapping;
    public class AuthProfiler:Profile
    {
        public AuthProfiler()
        {
            CreateMap<LoginRequest, LoginDto>(); 
            CreateMap<RegisterRequest, RegisterDto>();
            CreateMap<ConfirmEmailRequest, ConfirmEmailDto>();
        }
    }

