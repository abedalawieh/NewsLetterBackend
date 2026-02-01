using Microsoft.AspNetCore.Authorization;
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

        [HttpGet("categories")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAllCategories()
        {
            var categories = await _lookupService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("items/{category}")]
        public async Task<ActionResult<IEnumerable<LookupDto>>> GetByCategory(string category)
        {
            var items = await _lookupService.GetItemsByCategoryAsync(category);
            return Ok(items);
        }

        [HttpPut("items/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<LookupDto>> UpdateItem(Guid id, [FromBody] UpdateLookupDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var item = await _lookupService.UpdateItemAsync(id, dto);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpDelete("items/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteItem(Guid id)
        {
            return Forbid();
        }
    }
}
