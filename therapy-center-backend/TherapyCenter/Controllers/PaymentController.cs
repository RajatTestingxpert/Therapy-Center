using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TherapyCenter.DTO_s.Payment;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // GET api/payment/appointment/5
        [HttpGet("appointment/{appointmentId}")]
        [Authorize(Policy = "AllStaff")]
        public async Task<IActionResult> GetByAppointment(int appointmentId)
        {
            var payment = await _paymentService.GetByAppointmentAsync(appointmentId);
            return payment == null ? NotFound(new { message = "Payment not found." }) : Ok(payment);
        }

        // GET api/payment/patient/7
        [HttpGet("patient/{patientId}")]
        [Authorize(Policy = "AllStaff")]
        public async Task<IActionResult> GetByPatient(int patientId)
            => Ok(await _paymentService.GetByPatientAsync(patientId));

        // POST api/payment
        [HttpPost]
        [Authorize(Policy = "AllStaff")]
        public async Task<IActionResult> Record([FromBody] RecordPaymentRequest request)
        {
            try
            {
                return Ok(await _paymentService.RecordPaymentAsync(request));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // PATCH api/payment/5/paid
        [HttpPatch("{appointmentId}/paid")]
        [Authorize(Policy = "AllStaff")]
        public async Task<IActionResult> MarkPaid(int appointmentId, [FromBody] MarkPaymentPaidRequest request)
        {
            try
            {
                return Ok(await _paymentService.MarkAsPaidAsync(appointmentId, request.TransactionId));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}