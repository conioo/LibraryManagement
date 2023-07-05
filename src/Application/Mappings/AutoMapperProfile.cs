using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Dtos.Response.Archive;
using Application.Mappings.AutoMapperHelpers;
using AutoMapper;
using Domain.Entities;
using Profile = Domain.Entities.Profile;

namespace Application.Mappings
{
    internal class AutoMapperProfile : AutoMapper.Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<ItemRequest, Item>()
                 .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<UpdateItemRequest, Item>()
                 .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<LibraryRequest, Library>()
                 .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<ItemRequest, ItemResponse>();
            CreateMap<Item, ItemResponse>();

            CreateMap<UpdateItemRequest, ItemRequest>();

            CreateMap<Library, LibraryResponse>();

            CreateMap<Rental, RentalResponse>()
               .ForMember(dest => dest.ItemTitle, conf => conf.MapFrom(src => src.Copy.Item.Title));

            CreateMap<Reservation, ReservationResponse>()
               .ForMember(dest => dest.ItemTitle, conf => conf.MapFrom(src => src.Copy.Item.Title));

            CreateMap<Profile, ProfileResponse>()
                .ForMember(dest => dest.ProfileHistory, conf => conf.ExplicitExpansion());

            CreateMap<ArchivalRental, ArchivalRentalResponse>()
               .ForMember(dest => dest.ItemTitle, conf => conf.MapFrom(src => src.CopyHistory.Copy.Item.Title))
               .ForMember(dest => dest.ProfileLibraryCardNumber, conf => conf.MapFrom(src => src.ProfileHistory.Profile.LibraryCardNumber))
               .ForMember(dest => dest.CopyInventoryNumber, conf => conf.MapFrom(src => src.CopyHistory.Copy.InventoryNumber));

            CreateMap<Rental, ArchivalRental>();

            CreateMap<ArchivalReservation, ArchivalReservationResponse>()
               .ForMember(dest => dest.ItemTitle, conf => conf.MapFrom(src => src.CopyHistory.Copy.Item.Title))
               .ForMember(dest => dest.ProfileLibraryCardNumber, conf => conf.MapFrom(src => src.ProfileHistory.Profile.LibraryCardNumber))
               .ForMember(dest => dest.CopyInventoryNumber, conf => conf.MapFrom(src => src.CopyHistory.Copy.InventoryNumber));

            CreateMap<ProfileHistory, ProfileHistoryResponse>();
            CreateMap<CopyHistory, CopyHistoryResponse>();//tutaj

            CreateMap<Copy, CopyResponse>();

            CreateMap<Item, CopiesHelper>();

            CreateMap<int?, int>().ConvertUsing((src, dest) => src ?? dest);
            CreateMap<bool?, bool>().ConvertUsing((src, dest) => src ?? dest);
        }
    }
}