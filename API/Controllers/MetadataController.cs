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
    public class MetadataController : ControllerBase
    {
        private readonly ILookupService _lookupService;

        public MetadataController(ILookupService lookupService)
        {
            _lookupService = lookupService ?? throw new ArgumentNullException(nameof(lookupService));
        }

        [HttpGet("items/{category}")]
        public async Task<ActionResult<IEnumerable<LookupDto>>> GetByCategory(string category)
        {
            try
            {
                var items = await _lookupService.GetItemsByCategoryAsync(category);
                return Ok(items);
            }
            catch
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

    }
}
