using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Dtos.Response.Archive;
using Application.Mappings.AutoMapperHelpers;
using Domain.Entities;
using Profile = Domain.Entities.Profile;

namespace Application.Mappings
{
    internal class AutoMapperProfile : AutoMapper.Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<ItemRequest, Item>();
            CreateMap<ItemRequest, ItemResponse>();
            CreateMap<Item, ItemResponse>();

            CreateMap<Library, LibraryResponse>();
            CreateMap<LibraryRequest, Library>();

            CreateMap<Rental, RentalResponse>()
               .ForMember(dest => dest.ItemTitle, conf => conf.MapFrom(src => src.Copy.Item.Title));

            CreateMap<Reservation, ReservationResponse>()
               .ForMember(dest => dest.ItemTitle, conf => conf.MapFrom(src => src.Copy.Item.Title));

            CreateMap<Profile, ProfileResponse>()
                .ForMember(dest => dest.ProfileHistory, conf => conf.ExplicitExpansion());

            CreateMap<ArchivalRental, ArchivalRentalResponse>()
               .ForMember(dest => dest.ItemTitle, conf => conf.MapFrom(src => src.Copy != null ? src.Copy.Item.Title : null));

            CreateMap<ArchivalReservation, ArchivalReservationResponse>()
               .ForMember(dest => dest.ItemTitle, conf => conf.MapFrom(src => src.Copy != null ? src.Copy.Item.Title : null));

            CreateMap<ProfileHistory, ProfileHistoryResponse>();

            CreateMap<Copy, CopyResponse>();

            CreateMap<Item, CopiesHelper>();
        }

    }
}