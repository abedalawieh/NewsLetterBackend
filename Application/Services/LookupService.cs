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

        public async Task<LookupDto> CreateItemAsync(CreateLookupDto dto)
        {
            var category = await _repository.GetCategoryByNameAsync(dto.Category);
            if (category == null) throw new Exception("Category not found");

            // Newly created items via UI are NOT system items
            var item = LookupItem.Create(category.Id, dto.Value, dto.Label, dto.SortOrder, isSystem: false);
            var created = await _repository.AddItemAsync(item);
            return TranslateToDto(created);
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

        public async Task<bool> DeleteItemAsync(Guid id)
        {
            return await _repository.DeleteItemAsync(id);
        }

        public async Task<CategoryDto> CreateCategoryAsync(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Category name is required");

            var existing = await _repository.GetCategoryByNameAsync(name);
            if (existing != null) throw new InvalidOperationException($"Category '{name}' already exists.");

            // Newly created categories via UI are NOT system categories
            var category = LookupCategory.Create(name, description, isSystem: false);
            var created = await _repository.AddCategoryAsync(category);
            return TranslateCategoryToDto(created);
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            return await _repository.DeleteCategoryAsync(id);
        }

        public async Task<bool> CategoryHasItemsAsync(Guid categoryId)
        {
            var count = await _repository.GetItemCountByCategoryIdAsync(categoryId);
            return count > 0;
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
