using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsletterApp.API.Areas.Admin.Pages.ViewModels;
using NewsletterApp.Application.DTOs;
using NewsletterApp.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsletterApp.API.Areas.Admin.Pages.Metadata
{
    public class IndexModel : PageModel
    {
        private readonly ILookupService _lookupService;

        public IndexModel(ILookupService lookupService)
        {
            _lookupService = lookupService;
        }

        [BindProperty]
        public UpdateLookupDto EditItem { get; set; }
        
        [BindProperty]
        public Guid EditItemId { get; set; }

        public IEnumerable<CategoryDto> Categories { get; set; }
        public PageHeaderViewModel PageHeader { get; set; }

        public async Task OnGetAsync()
        {
            Categories = await _lookupService.GetAllCategoriesAsync();
            
            // Initialize page header
            PageHeader = new PageHeaderViewModel
            {
                Title = "Metadata Management",
                Subtitle = "Configure categories and lookup values for system operations.",
                Icon = "fas fa-cogs"
            };
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            ModelState.Clear();
            if (string.IsNullOrEmpty(EditItem.Label))
            {
                TempData["Error"] = "Label is required.";
                return RedirectToPage();
            }

            await _lookupService.UpdateItemAsync(EditItemId, EditItem);
            TempData["Success"] = "Item updated successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(Guid id, string category)
        {
            var items = await _lookupService.GetItemsByCategoryAsync(category);
            var item = items.FirstOrDefault(x => x.Id == id);
            
            if (item == null) return NotFound();

            var updateDto = new UpdateLookupDto 
            { 
                Label = item.Label,
                SortOrder = item.SortOrder,
                IsActive = !item.IsActive 
            };

            await _lookupService.UpdateItemAsync(id, updateDto);

            return RedirectToPage();
        }
    }
}
