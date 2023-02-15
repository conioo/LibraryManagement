using Application.Dtos;

namespace Application.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(Email message);
    }
}
