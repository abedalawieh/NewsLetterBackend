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
        public CreateLookupDto NewItem { get; set; }

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

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                Categories = await _lookupService.GetAllCategoriesAsync();
                var errorMsg = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)));
                TempData["Error"] = string.IsNullOrEmpty(errorMsg) ? "Please fill in all required fields." : errorMsg;
                return RedirectToPage();
            }

            try
            {
                await _lookupService.CreateItemAsync(NewItem);
                TempData["Success"] = $"'{NewItem.Label}' has been added successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to create item. Please try again.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            // We only validate the relevant fields for edit
            if (string.IsNullOrEmpty(EditItem.Label))
            {
                Categories = await _lookupService.GetAllCategoriesAsync();
                return Page();
            }

            await _lookupService.UpdateItemAsync(EditItemId, EditItem);
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
        [BindProperty]
        public string NewCategoryName { get; set; }
        
        [BindProperty]
        public string NewCategoryDescription { get; set; }

        public async Task<IActionResult> OnPostCreateCategoryAsync()
        {
            if (string.IsNullOrWhiteSpace(NewCategoryName))
            {
                Categories = await _lookupService.GetAllCategoriesAsync();
                return Page();
            }

            await _lookupService.CreateCategoryAsync(NewCategoryName, NewCategoryDescription);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            await _lookupService.DeleteItemAsync(id);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteCategoryAsync(Guid id)
        {
            try
            {
                await _lookupService.DeleteCategoryAsync(id);
                TempData["Success"] = "Category deleted successfully!";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToPage();
        }
    }
}
