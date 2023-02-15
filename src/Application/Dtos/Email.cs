#pragma warning disable CS8618 

namespace Application.Dtos
{
    public class Email
    {
        public Email(string to, string subject, string body)
        {
            To = to;
            Subject = subject;
            Body = body;
        }

        public string To { get; set; }
        public string? From { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
