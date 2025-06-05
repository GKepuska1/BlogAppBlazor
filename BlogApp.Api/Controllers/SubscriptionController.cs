using BlogApp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace BlogApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController, Authorize]
    public class SubscriptionController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public SubscriptionController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpGet("check")]
        public async Task<IActionResult> CheckLimit()
        {
            var user = await _userManager.FindByNameAsync(User.Identity!.Name);
            if (user == null)
                return Unauthorized();

            var limitReached = !user.SubscriptionActive && user.LastPostDate.Date == DateTime.UtcNow.Date && user.PostCount >= 1;
            return Ok(new { limitReached, user.SubscriptionActive });
        }

        [HttpPost("create-session")]
        public IActionResult CreateCheckoutSession()
        {
            var domain = _configuration["AppUrl"] ?? string.Empty;
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
            
            var options = new SessionCreateOptions
            {
                Mode = "subscription",
                SuccessUrl = domain + "/success",
                CancelUrl = domain + "/cancel",
                ClientReferenceId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = _configuration["Stripe:PriceId"],
                        Quantity = 1
                    }
                }
            };
            var service = new SessionService();
            var session = service.Create(options);
            return Ok(new { sessionId = session.Id, url = session.Url });
        }

        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], _configuration["Stripe:WebhookSecret"]);
            if (stripeEvent.Type == Events.CheckoutSessionCompleted)
            {
                var session = stripeEvent.Data.Object as Session;
                var userId = session?.ClientReferenceId;
                if (!string.IsNullOrEmpty(userId))
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        user.SubscriptionActive = true;
                        await _userManager.UpdateAsync(user);
                    }
                }
            }
            return Ok();
        }
    }
}
