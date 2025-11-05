using BlogApp.Core.Data;
using BlogApp.Core.Services;
using BlogApp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace BlogApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController, Authorize]
    public class SubscriptionController : ControllerBase
    {
        private readonly IRedisUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IBitcoinPaymentService _bitcoinPaymentService;

        public SubscriptionController(
            IRedisUserRepository userRepository,
            IConfiguration configuration,
            IBitcoinPaymentService bitcoinPaymentService)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _bitcoinPaymentService = bitcoinPaymentService;
        }

        [HttpGet("check")]
        public async Task<IActionResult> CheckLimit()
        {
            var user = await _userRepository.GetByUsernameAsync(User.Identity!.Name);
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
                    var user = await _userRepository.GetByIdAsync(userId);
                    if (user != null)
                    {
                        user.SubscriptionActive = true;
                        await _userRepository.UpdateAsync(user);
                    }
                }
            }
            return Ok();
        }

        // Bitcoin Payment Endpoints
        [HttpGet("bitcoin/info")]
        public IActionResult GetBitcoinPaymentInfo()
        {
            return Ok(new
            {
                address = _bitcoinPaymentService.GetPaymentAddress(),
                amount = _bitcoinPaymentService.GetSubscriptionPrice(),
                currency = "BTC"
            });
        }

        [HttpPost("bitcoin/verify")]
        public async Task<IActionResult> VerifyBitcoinPayment([FromBody] BitcoinPaymentVerification payment)
        {
            if (string.IsNullOrWhiteSpace(payment.TransactionId))
            {
                return BadRequest(new { message = "Transaction ID is required" });
            }

            var user = await _userRepository.GetByUsernameAsync(User.Identity!.Name);
            if (user == null)
                return Unauthorized();

            // Check if user already has active subscription
            if (user.SubscriptionActive)
            {
                return BadRequest(new { message = "You already have an active subscription" });
            }

            // Verify the Bitcoin transaction
            var isValid = await _bitcoinPaymentService.VerifyPayment(payment.TransactionId, user.Id);

            if (!isValid)
            {
                return BadRequest(new { message = "Invalid or unconfirmed transaction. Please ensure you sent the correct amount to the correct address." });
            }

            // Activate subscription
            user.SubscriptionActive = true;
            user.PostCount = 0; // Reset daily post count
            await _userRepository.UpdateAsync(user);

            return Ok(new {
                message = "Payment verified! Your subscription is now active.",
                subscriptionActive = true
            });
        }
    }

    public class BitcoinPaymentVerification
    {
        public string TransactionId { get; set; } = string.Empty;
    }
}
