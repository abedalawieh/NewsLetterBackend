using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewsletterApp.Application.Interfaces
{
    public interface IEmailService
    {
        /// <summary>
        /// Sends a basic email
        /// </summary>
        Task SendEmailAsync(string to, string subject, string body);

        /// <summary>
        /// Sends a newsletter email using the generic template
        /// </summary>
        Task SendNewsletterAsync(string to, string subject, string content);

        /// <summary>
        /// Sends a newsletter email using a specific template with personalization
        /// </summary>
        /// <param name="to">Recipient email address</param>
        /// <param name="firstName">Recipient first name for personalization</param>
        /// <param name="subject">Email subject</param>
        /// <param name="content">Newsletter content</param>
        /// <param name="templateName">Name of the template to use (null for generic)</param>
        Task SendNewsletterWithTemplateAsync(string to, string firstName, string subject, string content, string templateName = null);

        /// <summary>
        /// Gets the list of available email templates
        /// </summary>
        IEnumerable<string> GetAvailableTemplates();
    }
}
