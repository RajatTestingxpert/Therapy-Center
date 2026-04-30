using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TherapyCenter.DTO_s.Appointment;
using TherapyCenter.Services.Interfaces;
using System.Security.Claims;

namespace TherapyCenter.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IPatientService _patientService;

        public AppointmentController(IAppointmentService appointmentService, IPatientService patientService)
        {
            _appointmentService = appointmentService;
            _patientService = patientService;
        }

        // POST api/appointment/book
        // Receptionist or Admin books offline — ReceptionistId populated in request body
        [HttpPost("book")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> Book([FromBody] BookAppointmentRequest request)
        {
            try
            {
                return Ok(await _appointmentService.BookAsync(request));
            }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        // POST api/appointment/book-online
        // Patient or Guardian books themselves — ReceptionistId is forced null
        [HttpPost("book-online")]
        [Authorize(Policy = "PatientAccess")]
        public async Task<IActionResult> BookOnline([FromBody] BookAppointmentRequest request)
        {
            request.ReceptionistId = null;

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (request.PatientId <= 0 && string.Equals(role, "Patient", StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(userIdClaim, out var userId))
                    return Unauthorized(new { message = "Invalid token." });

                var patient = await _patientService.GetOrCreateByUserIdAsync(userId);
                request.PatientId = patient.PatientId;
            }

            try
            {
                return Ok(await _appointmentService.BookAsync(request));
            }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        // GET api/appointment/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var appt = await _appointmentService.GetByIdAsync(id);
            return appt == null ? NotFound() : Ok(appt);
        }

        // GET api/appointment/patient/5
        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetByPatient(int patientId)
            => Ok(await _appointmentService.GetByPatientAsync(patientId));

        // GET api/appointment/doctor/3
        [HttpGet("doctor/{doctorId}")]
        [Authorize(Policy = "AllStaff")]
        public async Task<IActionResult> GetByDoctor(int doctorId)
            => Ok(await _appointmentService.GetByDoctorAsync(doctorId));

        // GET api/appointment/date/2025-06-15
        [HttpGet("date/{date}")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> GetByDate(DateOnly date)
            => Ok(await _appointmentService.GetByDateAsync(date));

        // PATCH api/appointment/4/status
        [HttpPatch("{id}/status")]
        [Authorize(Policy = "AllStaff")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateAppointmentStatusRequest request)
        {
            try
            {
                return Ok(await _appointmentService.UpdateStatusAsync(id, request));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}