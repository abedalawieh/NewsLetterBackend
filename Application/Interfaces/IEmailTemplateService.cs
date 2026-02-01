using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewsletterApp.Application.Interfaces
{
    public interface IEmailTemplateService
    {
        IEnumerable<string> GetAvailableTemplates();

        Task<string> RenderTemplateAsync(string templateName, Dictionary<string, string> context);

        string GetTemplateNameForInterest(string interest);

        string GetBestTemplateName(string explicitTemplate, IEnumerable<string> interests);

        bool TemplateExists(string templateName);
    }
}
