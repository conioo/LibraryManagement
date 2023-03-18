using Application.Dtos;
using Application.Interfaces;
using Domain.Settings;
using MailKit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Application.Services
{
    internal class EmailService : IEmailService
    {
        private readonly MailSettings _mailOptions;
        private readonly IMailTransport _transportService;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<MailSettings> mailOptions, IMailTransport transportService, ILogger<EmailService> logger)
        {
            _mailOptions = mailOptions.Value;
            _transportService = transportService;
            _logger = logger;
        }
        public async Task SendAsync(Email dto)
        {
            var email = new MimeMessage();

            email.Sender = new MailboxAddress(_mailOptions.DisplayName, dto.From ?? _mailOptions.EmailAddress);
            email.To.Add(MailboxAddress.Parse(dto.To));
            email.Subject = dto.Subject;

            var builder = new BodyBuilder();
            builder.HtmlBody = dto.Body;
            email.Body = builder.ToMessageBody();

            await _transportService.ConnectAsync(_mailOptions.Host, _mailOptions.Port);
            _transportService.Authenticate(_mailOptions.Username, _mailOptions.Password);
            _transportService.Send(email);
            _transportService.Disconnect(true);

            _logger.LogInformation($"Email was sent to {dto.To}");
        }

    }
}