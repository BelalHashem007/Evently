namespace EventBookingSystem.Services.Interfaces
{
    public interface IEmailService
    {
        public Task SendEmailAsync(string mailTo, string subject, string body, IList<IFormFile>? attachments = null);
    }
}
