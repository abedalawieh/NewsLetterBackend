using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsletterApp.Application.Interfaces;
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
                    IsDraft = n.IsDraft,
                    SentAt = n.SentAt,
                    CreatedAt = n.CreatedAt,
                });
            }
            return Ok(result);
        }
    }

    public class NewsletterListDto
    {
        public System.Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string TargetInterests { get; set; } = "";
        public bool IsDraft { get; set; }
        public System.DateTime? SentAt { get; set; }
        public System.DateTime CreatedAt { get; set; }
    }
}
