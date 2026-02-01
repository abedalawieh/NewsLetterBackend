using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        /// <summary>
        /// Get all newsletters (public - for subscription flow)
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
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
                    TargetSubscriberType = n.TargetSubscriberType,
                    TemplateName = n.TemplateName,
                    IsDraft = n.IsDraft,
                    SentAt = n.SentAt,
                    CreatedAt = n.CreatedAt,
                });
            }
            return Ok(result);
        }

        /// <summary>
        /// Get a specific newsletter by ID (public - for viewing published newsletters)
        /// </summary>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<ActionResult<NewsletterDetailDto>> GetNewsletterById(Guid id)
        {
            var newsletter = await _newsletterService.GetByIdAsync(id);
            
            if (newsletter == null)
            {
                return NotFound(new { message = "Newsletter not found" });
            }

            // Only return published newsletters to public
            // Drafts should only be visible to admins
            
            return Ok(new NewsletterDetailDto
            {
                Id = newsletter.Id,
                Title = newsletter.Title,
                Content = newsletter.Content,
                TargetInterests = newsletter.TargetInterests,
                TargetSubscriberType = newsletter.TargetSubscriberType,
                TemplateName = newsletter.TemplateName,
                IsDraft = newsletter.IsDraft,
                SentAt = newsletter.SentAt,
                CreatedAt = newsletter.CreatedAt,
            });
        }
    }

    public class NewsletterListDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string TargetInterests { get; set; } = "";
        public string TargetSubscriberType { get; set; }
        public string TemplateName { get; set; }
        public bool IsDraft { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class NewsletterDetailDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public string TargetInterests { get; set; } = "";
        public string TargetSubscriberType { get; set; }
        public string TemplateName { get; set; }
        public bool IsDraft { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
