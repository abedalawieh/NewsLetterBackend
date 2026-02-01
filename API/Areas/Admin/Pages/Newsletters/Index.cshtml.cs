using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp.API.Areas.Admin.Pages.ViewModels;
using NewsletterApp.Application.Interfaces;
using NewsletterApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsletterApp.API.Areas.Admin.Pages.Newsletters
{
    public class IndexModel : PageModel
    {
        private readonly INewsletterService _newsletterService;

        public IndexModel(INewsletterService newsletterService)
        {
            _newsletterService = newsletterService;
        }

        public IEnumerable<Newsletter> Newsletters { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public PaginationViewModel Pagination { get; set; }

        public async Task OnGetAsync()
        {
            Newsletters = await _newsletterService.GetHistoryAsync();
            var newsletterList = Newsletters.ToList();
            
            // Calculate pagination
            var totalItems = newsletterList.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            Newsletters = newsletterList.Skip((PageNumber - 1) * PageSize).Take(PageSize).ToList();

            // Initialize pagination
            Pagination = new PaginationViewModel
            {
                CurrentPage = PageNumber,
                TotalPages = totalPages,
                TotalItems = totalItems,
                PageSize = PageSize,
                PageParameterName = "pageNumber"
            };
        }

        public async Task<IActionResult> OnPostSendAsync(Guid id)
        {
            try
            {
                await _newsletterService.SendNewsletterAsync(id);
                TempData["SuccessMessage"] = "Newsletter sent successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error sending newsletter: {ex.Message}";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var result = await _newsletterService.DeleteAsync(id);
                TempData["SuccessMessage"] = result ? "Newsletter deleted successfully." : "Newsletter not found.";
                if (!result) TempData["ErrorMessage"] = "Newsletter not found.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting newsletter: {ex.Message}";
            }
            return RedirectToPage();
        }
    }
}