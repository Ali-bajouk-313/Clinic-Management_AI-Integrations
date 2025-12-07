using ClinicManagement.Data;
using ClinicManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace ClinicManagement.Controllers
{
    [Authorize] // Only logged in users can pay
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public PaymentController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // 1) Create Stripe Checkout session and redirect to Stripe
        public async Task<IActionResult> CreateCheckoutSession(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
                return NotFound();

            // TODO: replace with dynamic price if you store it in DB
            var amount = 50m; // 50 USD for example

            var domain = $"{Request.Scheme}://{Request.Host}";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                Mode = "payment",
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            UnitAmount = (long)(amount * 100), // amount in cents
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Appointment for {appointment.Patient.Name}"
                            }
                        }
                    }
                },
                SuccessUrl = $"{domain}/Payment/Success?appointmentId={appointmentId}",
                CancelUrl = $"{domain}/Payment/Cancel?appointmentId={appointmentId}"
            };

            var service = new SessionService();
            Session session = service.Create(options);

            return Redirect(session.Url);
        }

        // 2) When payment succeeded
        public async Task<IActionResult> Success(int appointmentId)
        {
            var payment = new Payment
            {
                AppointmentId = appointmentId,
                Amount = 50m,
                PaymentMethod = "Stripe",
                Status = "Paid"
            };
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Online payment completed successfully via Stripe.";
            return RedirectToAction("Index", "Appointment");
        }


        // 3) When user cancels payment
        public IActionResult Cancel(int appointmentId)
        {
            TempData["Error"] = "Payment was cancelled.";
            return RedirectToAction("Index", "Appointment");
        }
    }
}
