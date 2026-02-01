using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewsletterApp.Application.DTOs;

namespace NewsletterApp.Application.Interfaces
{
    public interface ILookupService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
        Task<IEnumerable<LookupDto>> GetItemsByCategoryAsync(string category);
        Task<LookupDto> UpdateItemAsync(Guid id, UpdateLookupDto dto);
    }

    public class CategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsSystem { get; set; }
        public List<LookupDto> Items { get; set; }
    }
}
