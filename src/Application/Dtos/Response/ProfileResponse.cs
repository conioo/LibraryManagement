﻿#pragma warning disable CS8618

using Application.Dtos.Response.Archive;
using Domain.Entities;

namespace Application.Dtos.Response
{
    public class ProfileResponse
    {
        public string LibraryCardNumber { get; set; }
        public string UserId { get; set; }

        public bool IsActive { get; set; }

        public ICollection<RentalResponse>? CurrentRentals { get; set; }
        public ICollection<ReservationResponse>? CurrentReservations { get; set; }

        public ProfileHistoryResponse? ProfileHistory { get; set; }
    }
}
