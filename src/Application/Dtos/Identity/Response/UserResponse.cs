﻿#pragma warning disable CS8618

namespace Application.Dtos.Identity.Response
{
    public class UserResponse
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime Created { get; set; }
    }
}
