using Microsoft.Extensions.Configuration;
using NewsletterApp.Application.Interfaces;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace NewsletterApp.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var smtpSettings = _config.GetSection("EmailSettings");
            
            using var client = new SmtpClient(smtpSettings["Host"])
            {
                Port = int.Parse(smtpSettings["Port"]),
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
        }

        public async Task SendNewsletterAsync(string to, string subject, string content)
        {
            var frontendUrl = _config["EmailSettings:FrontendBaseUrl"];
            var unsubscribeLink = $"{frontendUrl}/unsubscribe?email={Uri.EscapeDataString(to)}";

            // Professional HTML Template
            string htmlTemplate = $@"
                <div style='font-family: ""Segoe UI"", Tahoma, Geneva, Verdana, sans-serif; max-width: 600px; margin: auto; background-color: #ffffff; border: 1px solid #e2e8f0; border-radius: 8px; overflow: hidden;'>
                    <!-- Header -->
                    <div style='background-color: #6366f1; padding: 30px; text-align: center;'>
                        <h1 style='color: white; margin: 0; font-size: 24px; font-weight: 600;'>Newsletter Update</h1>
                    </div>

                    <!-- Body -->
                    <div style='padding: 40px 30px; line-height: 1.6; color: #334155; font-size: 16px;'>
                        {content}
                    </div>

                    <!-- Action -->
                    <div style='text-align: center; padding: 20px 30px; background-color: #f8fafc;'>
                        <p style='color: #64748b; font-size: 14px; margin-bottom: 20px;'>
                            Not interested specifically in this topic? You can update your preferences or unsubscribe below.
                        </p>
                        <a href='{unsubscribeLink}' style='background-color: #ef4444; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: 500; font-size: 14px; display: inline-block;'>
                            Unsubscribe
                        </a>
                    </div>

                    <!-- Footer -->
                    <div style='padding: 20px; background-color: #f1f5f9; text-align: center; font-size: 12px; color: #94a3b8;'>
                        <p style='margin: 0;'>
                            &copy; {System.DateTime.Now.Year} Newsletter App. All rights reserved.<br>
                            You received this email because you signed up on our website.
                        </p>
                    </div>
                </div>";

            await SendEmailAsync(to, subject, htmlTemplate);
        }
    }
}

