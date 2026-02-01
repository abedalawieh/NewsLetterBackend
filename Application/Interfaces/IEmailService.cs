using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewsletterApp.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);

        Task SendNewsletterAsync(string to, string subject, string content);

        Task SendNewsletterWithTemplateAsync(
            string to,
            string firstName,
            string lastName,
            string type,
            IEnumerable<string> communicationMethods,
            IEnumerable<string> interests,
            string subject,
            string content,
            string templateName = null);

        IEnumerable<string> GetAvailableTemplates();
    }
}
