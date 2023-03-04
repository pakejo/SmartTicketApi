using AutoMapper;
using SmartTicketApi.Data.DTO;
using SmartTicketApi.Models;

namespace SmartTicketApi.Utilities
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            AllowNullCollections = true;

            _ = CreateMap<EventCreationDto, Event>();

            _ = CreateMap<Event, EventDto>()
                .ForMember(dto => dto.Sales, opts => opts.MapFrom(src => src.Sales.Select(s => s.Id)));

            _ = CreateMap<UserCreationDto, UserCredentialsDto>();

            _ = CreateMap<Sale, SaleDto>();
        }
    }
}
