using Microsoft.AspNetCore.Mvc;
using NewsletterApp.Application.DTOs;
using NewsletterApp.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace NewsletterApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SubscribersController : ControllerBase
    {
        private readonly ISubscriberService _subscriberService;

        public SubscribersController(ISubscriberService subscriberService)
        {
            _subscriberService = subscriberService ?? throw new ArgumentNullException(nameof(subscriberService));
        }

        [HttpPost]
        [ProducesResponseType(typeof(SubscriberResponseDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        public async Task<ActionResult<SubscriberResponseDto>> CreateSubscriber([FromBody] CreateSubscriberDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var subscriber = await _subscriberService.CreateSubscriberAsync(dto);
                return StatusCode(201, subscriber);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.StartsWith("WELCOME_BACK:"))
                {
                    var name = ex.Message.Split(':')[1];
                    return Ok(new { message = $"Welcome back, {name}! Your subscription has been reactivated.", status = "reactivated" });
                }
                if (ex.Message == "ALREADY_ACTIVE")
                {
                    return Conflict(new { message = "This email is already subscribed to our newsletter.", status = "active" });
                }
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("unsubscribe")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try 
            {
                await _subscriberService.UnsubscribeAsync(dto.Email, dto.Reason, dto.Comment);
                return Ok(new { message = "You have been successfully unsubscribed." });
            }
            catch (KeyNotFoundException ex)
            {
                if (ex.Message == "NO_ACCOUNT")
                {
                    return NotFound(new { message = "There is no account associated with this email address." });
                }
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
