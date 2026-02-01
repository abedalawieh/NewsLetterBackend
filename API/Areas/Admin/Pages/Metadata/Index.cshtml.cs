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
            // Remove unrelated model errors to prevent cross-form validation issues
            ModelState.Clear();
            if (string.IsNullOrWhiteSpace(NewItem.Category) || string.IsNullOrWhiteSpace(NewItem.Value) || string.IsNullOrWhiteSpace(NewItem.Label))
            {
                TempData["Error"] = "Category, Code and Label are all required.";
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
        [BindProperty]
        public string? NewCategoryName { get; set; }
        
        [BindProperty]
        public string? NewCategoryDescription { get; set; }

        public async Task<IActionResult> OnPostCreateCategoryAsync()
        {
            if (string.IsNullOrWhiteSpace(NewCategoryName))
            {
                TempData["Error"] = "Category name is required.";
                return RedirectToPage();
            }

            try
            {
                await _lookupService.CreateCategoryAsync(NewCategoryName, NewCategoryDescription);
                TempData["Success"] = $"Category '{NewCategoryName}' created successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToPage();
        }

        private readonly string[] _systemCategories = { "SubscriberType", "CommunicationMethod", "Interest" };

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var categories = await _lookupService.GetAllCategoriesAsync();
            var item = categories.SelectMany(c => c.Items).FirstOrDefault(i => i.Id == id);
            
            if (item != null && item.IsSystem)
            {
                TempData["Error"] = $"Cannot delete '{item.Label}' because it is a system-defined item.";
                return RedirectToPage();
            }

            await _lookupService.DeleteItemAsync(id);
            TempData["Success"] = "Item deleted successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteCategoryAsync(Guid id)
        {
            try
            {
                var categories = await _lookupService.GetAllCategoriesAsync();
                var category = categories.FirstOrDefault(c => c.Id == id);
                
                if (category != null && category.IsSystem)
                {
                    TempData["Error"] = $"Cannot delete system category '{category.Name}'.";
                    return RedirectToPage();
                }

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
