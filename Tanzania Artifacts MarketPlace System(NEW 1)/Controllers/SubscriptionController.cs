using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromForm] string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email))
                return BadRequest("Invalid email address.");

            bool alreadyExists = await _context.Subscribers.AnyAsync(s => s.Email == email);
            if (alreadyExists)
                return Conflict("You are already subscribed.");

            var subscriber = new Subscriber { Email = email };
            _context.Subscribers.Add(subscriber);
            await _context.SaveChangesAsync();

            return Ok("Thank you for subscribing!");
        }
    }

}
