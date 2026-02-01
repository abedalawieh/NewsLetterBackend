using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewsletterApp.Application.Interfaces
{
    /// <summary>
    /// Service for managing and rendering email templates
    /// Follows the Template Method pattern for consistent email generation
    /// </summary>
    public interface IEmailTemplateService
    {
        /// <summary>
        /// Gets the list of available template names
        /// </summary>
        IEnumerable<string> GetAvailableTemplates();

        /// <summary>
        /// Renders an email template with the provided context data
        /// </summary>
        /// <param name="templateName">Name of the template (without extension)</param>
        /// <param name="context">Dictionary of placeholder values</param>
        /// <returns>Rendered HTML content</returns>
        Task<string> RenderTemplateAsync(string templateName, Dictionary<string, string> context);

        /// <summary>
        /// Gets the template name based on interest type
        /// </summary>
        string GetTemplateNameForInterest(string interest);

        /// <summary>
        /// Determines the best template name for a specific recipient given an optional explicit template,
        /// and their interests. This enables per-recipient automatic template selection.
        /// </summary>
        string GetBestTemplateName(string explicitTemplate, IEnumerable<string> interests);

        /// <summary>
        /// Checks if a template exists
        /// </summary>
        bool TemplateExists(string templateName);
    }
}
