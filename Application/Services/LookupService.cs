using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NewsletterApp.Application.DTOs;
using NewsletterApp.Application.Interfaces;
using NewsletterApp.Domain.Entities;
using NewsletterApp.Domain.Interfaces;

namespace NewsletterApp.Application.Services
{
    public class LookupService : ILookupService
    {
        private readonly ILookupRepository _repository;

        public LookupService(ILookupRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _repository.GetAllCategoriesAsync();
            return categories.Select(TranslateCategoryToDto);
        }

        public async Task<IEnumerable<LookupDto>> GetItemsByCategoryAsync(string category)
        {
            var items = await _repository.GetItemsByCategoryAsync(category);
            return items.Select(TranslateToDto);
        }

        public async Task<LookupDto> UpdateItemAsync(Guid id, UpdateLookupDto dto)
        {
            var item = await _repository.GetItemByIdAsync(id);
            if (item == null) return null;

            item.Label = dto.Label;
            item.SortOrder = dto.SortOrder;
            item.IsActive = dto.IsActive;

            var updated = await _repository.UpdateItemAsync(item);
            return TranslateToDto(updated);
        }

        private CategoryDto TranslateCategoryToDto(LookupCategory category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsSystem = category.IsSystem || new[] { "SubscriberType", "CommunicationMethod", "Interest" }.Contains(category.Name),
                Items = category.Items?.Select(TranslateToDto).ToList() ?? new List<LookupDto>()
            };
        }

        private LookupDto TranslateToDto(LookupItem item)
        {
            return new LookupDto
            {
                Id = item.Id,
                Value = item.Value,
                Label = item.Label,
                SortOrder = item.SortOrder,
                IsActive = item.IsActive,
                IsSystem = item.IsSystem
            };
        }
    }
}
