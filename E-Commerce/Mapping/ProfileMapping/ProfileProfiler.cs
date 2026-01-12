using E_Commerce.DTOS.User.Request;

namespace E_Commerce.Mapping.ProfileMapping;
    public class ProfileProfiler:Profile
    {
        public ProfileProfiler()
        {
            CreateMap<AppUser, UserRequest>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Full_Name))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.Job_Title));
        }
        }
    

