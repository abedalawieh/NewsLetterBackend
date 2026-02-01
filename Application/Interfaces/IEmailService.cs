using System.Threading.Tasks;

namespace NewsletterApp.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendNewsletterAsync(string to, string subject, string content);
    }
}
