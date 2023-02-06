using AutoMapper;
using PlatformService.Models;
using PlatformService.Dtos;

namespace PlatformService.Profiles{
    public class PlatformProfile:Profile{
        public PlatformProfile()
        {
            //source -> targe
            CreateMap<Platform,PlatformReadDto>();
            CreateMap<PlatformCreateDto, Platform>();
            CreateMap<PlatformReadDto, PlatformPublishedDto>();
            CreateMap<Platform,GrpcPlatformModel>()
                .ForMember(d=>d.PlatformId, opt=>opt.MapFrom(src=>src.Id))
                .ForMember(d=>d.Name, opt=>opt.MapFrom(src=>src.Name))
                .ForMember(d=>d.Publisher, opt=>opt.MapFrom(src=>src.Publisher));
        }
    }
}