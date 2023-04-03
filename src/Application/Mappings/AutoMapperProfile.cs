﻿using Application.Dtos.Request;
using Application.Dtos.Response;
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



            // pod mapowanie
            CreateMap<Profile, ProfileResponse>();
                //.ForMember(dest => dest.HistoryRentals, conf => conf.MapFrom(src => src.HistoryRentals));

           
        }
    }
}
