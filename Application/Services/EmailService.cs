using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewsletterApp.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace NewsletterApp.Application.Services
{
    /// <summary>
    /// Email Service implementation
    /// Uses the Strategy pattern for template selection
    /// Follows Single Responsibility Principle - focused only on email sending
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly IEmailTemplateService _templateService;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IConfiguration config, 
            IEmailTemplateService templateService,
            ILogger<EmailService> logger)
        {
            _config = config;
            _templateService = templateService;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var smtpSettings = _config.GetSection("EmailSettings");
            
            try
            {
                using var client = new SmtpClient(smtpSettings["Host"])
                {
                    Port = int.Parse(smtpSettings["Port"] ?? "587"),
                    Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpSettings["FromEmail"], smtpSettings["FromName"]),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", to);
                throw;
            }
        }

        public async Task SendNewsletterAsync(string to, string subject, string content)
        {
            await SendNewsletterWithTemplateAsync(to, "Subscriber", subject, content, null);
        }

        public async Task SendNewsletterWithTemplateAsync(
            string to, 
            string firstName, 
            string subject, 
            string content, 
            string templateName = null)
        {
            var frontendUrl = _config["EmailSettings:FrontendBaseUrl"] ?? "http://localhost:5173";
            var unsubscribeLink = $"{frontendUrl}/unsubscribe?email={Uri.EscapeDataString(to)}";

            // Use specified template or fall back to generic
            var template = string.IsNullOrWhiteSpace(templateName) ? "GenericNewsletter" : templateName;

            // Build the context dictionary for template rendering
            var context = new Dictionary<string, string>
            {
                { "Subject", subject },
                { "FirstName", firstName ?? "Subscriber" },
                { "Content", content },
                { "UnsubscribeLink", unsubscribeLink },
                { "Year", DateTime.Now.Year.ToString() }
            };

            try
            {
                var htmlContent = await _templateService.RenderTemplateAsync(template, context);
                await SendEmailAsync(to, subject, htmlContent);
                _logger.LogInformation("Newsletter sent to {Email} using template {Template}", to, template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send newsletter to {Email} using template {Template}", to, template);
                throw;
            }
        }

        public IEnumerable<string> GetAvailableTemplates()
        {
            return _templateService.GetAvailableTemplates();
        }
    }
}
