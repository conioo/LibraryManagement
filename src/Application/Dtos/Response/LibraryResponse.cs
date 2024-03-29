﻿#pragma warning disable CS8618
namespace Application.Dtos.Response
{
    public class LibraryResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }

        public int NumberOfComputerStations { get; set; }
        public bool IsPhotocopier { get; set; }
        public bool IsPrinter { get; set; }
        public bool IsScanner { get; set; }
    }
}
