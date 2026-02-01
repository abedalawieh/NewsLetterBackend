using Microsoft.AspNetCore.Mvc;
using NewsletterApp.Application.DTOs;
using NewsletterApp.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewsletterApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class NewslettersController : ControllerBase
    {
        private readonly INewsletterService _newsletterService;

        public NewslettersController(INewsletterService newsletterService)
        {
            _newsletterService = newsletterService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NewsletterListDto>>> GetNewsletters()
        {
            var newsletters = await _newsletterService.GetHistoryAsync();
            var result = new List<NewsletterListDto>();
            foreach (var n in newsletters)
            {
                result.Add(new NewsletterListDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    TargetInterests = n.TargetInterests,
                    TemplateName = n.TemplateName,
                    IsDraft = n.IsDraft,
                    SentAt = n.SentAt,
                    CreatedAt = n.CreatedAt,
                });
            }
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<NewsletterDetailDto>> GetNewsletterById(Guid id)
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
                TemplateName = newsletter.TemplateName,
                IsDraft = newsletter.IsDraft,
                SentAt = newsletter.SentAt,
                CreatedAt = newsletter.CreatedAt,
            });
        }
    }
}
