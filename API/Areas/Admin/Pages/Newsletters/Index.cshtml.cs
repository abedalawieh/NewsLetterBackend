using Microsoft.AspNetCore.Mvc;
using NewsletterApp.API.Areas.Admin.Pages.ViewModels;
using NewsletterApp.Application.Interfaces;
using NewsletterApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewsletterApp.API.Areas.Admin.Pages.Newsletters
{
    public class IndexModel : BasePaginatedPageModel
    {
        private readonly INewsletterService _newsletterService;

        public IndexModel(INewsletterService newsletterService)
        {
            _newsletterService = newsletterService;
        }

        public IEnumerable<Newsletter> Newsletters { get; set; }

        public PaginationViewModel Pagination { get; set; }

        public async Task OnGetAsync()
        {
            var (items, totalCount) = await _newsletterService.GetPagedHistoryAsync(PageNumber, PageSize);
            Newsletters = items;
            Pagination = BuildPagination(totalCount);
        }

        public async Task<IActionResult> OnPostSendAsync(Guid id)
        {
            try
            {
                await _newsletterService.SendNewsletterAsync(id);
                SetSuccess("Newsletter sent successfully!");
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message == "NO_RECIPIENTS")
                {
                    SetError("No subscribers match the selected interests.");
                }
                else if (ex.Message == "NOT_DRAFT")
                {
                    SetError("Newsletter not found or already sent.");
                }
                else if (ex.Message == "SEND_FAILED")
                {
                    SetError("No emails were delivered. Please check SMTP settings.");
                }
                else
                {
                    SetError("We couldn't send the newsletter. Please try again or contact support.");
                }
            }
            catch
            {
                SetError("We couldn't send the newsletter. Please try again or contact support.");
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var result = await _newsletterService.DeleteAsync(id);
                if (result)
                {
                    SetSuccess("Newsletter deleted successfully.");
                }
                else
                {
                    SetError("Newsletter not found.");
                }
            }
            catch
            {
                SetError("We couldn't delete the newsletter. Please try again or contact support.");
            }
            return RedirectToPage();
        }
    }
}
