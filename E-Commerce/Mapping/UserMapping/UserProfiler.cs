using E_Commerce.DTOS.Auth.Responses;
using E_Commerce.DTOS.User.Request;

namespace E_Commerce.Mapping.UserMapping;
    public class UserProfiler :Profile
    {
        public UserProfiler()
        {
        CreateMap<UserCreateRequest, AppUser>().ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.UserName,opt => opt.MapFrom(src => src.Email.GetUsernameFromEmail()));


        CreateMap<UserSearchRequest, AppUser>().ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

        CreateMap<UserUpdateRequest, AppUser>().ForMember(dest => dest.Full_Name, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.Job_Title, opt => opt.MapFrom(src => src.JobTitle));

        CreateMap<AppUser, UserResponse>();
    }
}

