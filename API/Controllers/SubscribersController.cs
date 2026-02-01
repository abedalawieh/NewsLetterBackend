using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsletterApp.Application.DTOs;
using NewsletterApp.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewsletterApp.API.Controllers
{
    /// <summary>
    /// RESTful API controller for subscriber management
    /// Following REST principles and best practices
    /// </summary>
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

        /// <summary>
        /// Get all subscribers
        /// </summary>
        /// <returns>List of subscribers</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(IEnumerable<SubscriberResponseDto>), 200)]
        public async Task<ActionResult<IEnumerable<SubscriberResponseDto>>> GetAllSubscribers()
        {

            var subscribers = await _subscriberService.GetAllSubscribersAsync();
            return Ok(subscribers);
        }

        /// <summary>
        /// Get subscriber by ID
        /// </summary>
        /// <param name="id">Subscriber ID</param>
        /// <returns>Subscriber details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SubscriberResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<SubscriberResponseDto>> GetSubscriberById(Guid id)
        {
            try
            {
                var subscriber = await _subscriberService.GetSubscriberByIdAsync(id);
                return Ok(subscriber);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Create a new subscriber
        /// </summary>
        /// <param name="dto">Subscriber creation data</param>
        /// <returns>Created subscriber</returns>
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
                return CreatedAtAction(
                    nameof(GetSubscriberById),
                    new { id = subscriber.Id },
                    subscriber
                );
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


        /// <summary>
        /// Delete a subscriber
        /// </summary>
        /// <param name="id">Subscriber ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteSubscriber(Guid id)
        {
            var result = await _subscriberService.DeleteSubscriberAsync(id);
            if (!result)
            {
                return NotFound(new { message = $"Subscriber with ID {id} not found." });
            }

            return NoContent();
        }

        /// <summary>
        /// Deactivate a subscriber
        /// </summary>
        /// <param name="id">Subscriber ID</param>
        /// <returns>No content</returns>
        [HttpPatch("{id}/deactivate")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeactivateSubscriber(Guid id)
        {
            try
            {
                await _subscriberService.DeactivateSubscriberAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Activate a subscriber
        /// </summary>
        /// <param name="id">Subscriber ID</param>
        /// <returns>No content</returns>
        [HttpPatch("{id}/activate")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ActivateSubscriber(Guid id)
        {
            try
            {
                await _subscriberService.ActivateSubscriberAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        /// <summary>
        /// Unsubscribe with a reason
        /// </summary>
        /// <param name="dto">Unsubscribe data</param>
        /// <returns>No content</returns>
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

