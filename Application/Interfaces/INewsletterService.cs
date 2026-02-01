using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewsletterApp.Domain.Entities;

namespace NewsletterApp.Application.Interfaces
{
    /// <summary>
    /// Service interface for newsletter operations
    /// Follows Interface Segregation Principle with focused methods
    /// </summary>
    public interface INewsletterService
    {
        /// <summary>
        /// Creates a new newsletter draft
        /// </summary>
        Task<Newsletter> CreateDraftAsync(string title, string content, List<string> interests);

        /// <summary>
        /// Sends a newsletter to subscribers matching the target criteria
        /// </summary>
        /// <param name="newsletterId">The newsletter ID to send</param>
        /// <param name="templateName">Optional specific template to use (null for auto-selection)</param>
        Task SendNewsletterAsync(Guid newsletterId, string templateName = null);
        Task<string> RenderTemplateForRecipientAsync(Guid newsletterId, Guid subscriberId, string templateName = null);

        /// <summary>
        /// Gets the newsletter history
        /// </summary>
        Task<IEnumerable<Newsletter>> GetHistoryAsync();

        /// <summary>
        /// Gets a single newsletter by ID
        /// </summary>
        Task<Newsletter> GetByIdAsync(Guid newsletterId);

        /// <summary>
        /// Gets paginated history
        /// </summary>
        Task<(IEnumerable<Newsletter> Items, int TotalCount)> GetPagedHistoryAsync(int pageNumber, int pageSize);

        /// <summary>
        /// Deletes a newsletter
        /// </summary>
        Task<bool> DeleteAsync(Guid newsletterId);
    }
}
