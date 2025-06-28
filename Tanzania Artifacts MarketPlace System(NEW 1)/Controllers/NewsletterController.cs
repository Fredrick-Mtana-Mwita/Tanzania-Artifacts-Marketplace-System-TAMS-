using Microsoft.AspNetCore.Mvc;


namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsletterController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NewsletterController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromForm] string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                return BadRequest("Invalid email.");

            bool exists = await _context.NewsletterSubscriptions
                .AnyAsync(n => n.Email == email);

            if (exists)
                return Conflict("Email already subscribed.");

            var subscription = new NewsletterSubscription
            {
                Email = email,
                SubscribedAt = DateTime.UtcNow
            };

            _context.NewsletterSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            return Ok("Subscribed successfully!");
        }
    }
}
