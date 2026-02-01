using Microsoft.AspNetCore.Mvc;
using NewsletterApp.Application.DTOs;
using NewsletterApp.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsletterApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class NewslettersController : ControllerBase
    {
        private readonly INewsletterService _newsletterService;
        private readonly ILookupService _lookupService;

        public NewslettersController(INewsletterService newsletterService, ILookupService lookupService)
        {
            _newsletterService = newsletterService;
            _lookupService = lookupService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<NewsletterListDto>>> GetNewsletters([FromQuery] NewsletterFilterParams filter)
        {
            try
            {
                var result = await _newsletterService.GetPagedPublishedAsync(filter);
                return Ok(result);
            }
            catch
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<NewsletterDetailDto>> GetNewsletterById(Guid id)
        {
            try
            {
                var newsletter = await _newsletterService.GetByIdAsync(id);

                if (newsletter == null)
                {
                    return NotFound(new { message = "Newsletter not found" });
                }

                return Ok(new NewsletterDetailDto
                {
                    Id = newsletter.Id,
                    Title = newsletter.Title,
                    Content = newsletter.Content,
                    TargetInterests = newsletter.TargetInterests,
                    TargetInterestLabels = await BuildInterestLabelsAsync(newsletter.TargetInterests),
                    TemplateName = newsletter.TemplateName,
                    IsDraft = newsletter.IsDraft,
                    SentAt = newsletter.SentAt,
                    CreatedAt = newsletter.CreatedAt,
                });
            }
            catch
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        private async Task<List<string>> BuildInterestLabelsAsync(string targetInterests)
        {
            var labels = new List<string>();
            if (string.IsNullOrWhiteSpace(targetInterests))
            {
                return labels;
            }

            var items = await _lookupService.GetItemsByCategoryAsync("Interest");
            var map = items
                .Where(i => i.IsActive)
                .GroupBy(i => i.Value, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().Label, StringComparer.OrdinalIgnoreCase);

            foreach (var value in targetInterests.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = value.Trim();
                if (trimmed.Length == 0) continue;
                labels.Add(map.TryGetValue(trimmed, out var label) ? label : trimmed);
            }

            return labels;
        }
    }
}
