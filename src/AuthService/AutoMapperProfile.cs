using AutoMapper;
using AuthService.DTOs;
using AuthService.Models;

namespace AuthService
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UserDTO, User>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.PersonaName))
                .ForMember(dest => dest.AvatarURL, opt => opt.MapFrom(src => src.Avatar))
                .ForMember(dest => dest.SteamID, opt => opt.MapFrom(src => src.SteamId))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone));

            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.PersonaName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.AvatarURL))
                .ForMember(dest => dest.SteamId, opt => opt.MapFrom(src => src.SteamID))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone));
        }
    }
}
