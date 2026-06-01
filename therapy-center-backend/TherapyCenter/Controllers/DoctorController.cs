using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TherapyCenter.DTO_s.Finding;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        private readonly IAppointmentService _appointmentService;
        private readonly IFindingService _findingService;

        public DoctorController(
            IDoctorService doctorService,
            IAppointmentService appointmentService,
            IFindingService findingService)
        {
            _doctorService = doctorService;
            _appointmentService = appointmentService;
            _findingService = findingService;
        }

        // GET api/doctor
        // All authenticated users can list doctors (needed for booking page)
        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _doctorService.GetAllAsync());

        // GET api/doctor/3
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var doctor = await _doctorService.GetByIdAsync(id);
            return doctor == null ? NotFound() : Ok(doctor);
        }

        // GET api/doctor/3/slots?date=2025-06-15
        // All authenticated users can see available slots (needed for booking)
        [HttpGet("{id}/slots")]
        public async Task<IActionResult> GetAvailableSlots(int id, [FromQuery] DateOnly date)
            => Ok(await _doctorService.GetAvailableSlotsAsync(id, date));

        // GET api/doctor/3/appointments
        // Staff and the doctor themselves can see a doctor's schedule
        [HttpGet("{id}/appointments")]
        [Authorize(Policy = "AllStaff")]
        public async Task<IActionResult> GetAppointments(int id)
            => Ok(await _appointmentService.GetByDoctorAsync(id));

        // GET api/doctor/my-appointments
        // Doctor reads their own UserId from the JWT — no URL parameter needed
        [HttpGet("my-appointments")]
        [Authorize(Policy = "DoctorOnly")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { message = "Invalid token." });

            var doctor = await _doctorService.GetByUserIdAsync(userId);

            if (doctor == null)
                return NotFound(new { message = "Doctor profile not found for this user." });

            return Ok(await _appointmentService.GetByDoctorAsync(doctor.DoctorId));
        }

        // GET api/doctor/appointments/15/finding
        [HttpGet("appointments/{appointmentId}/finding")]
        [Authorize(Policy = "DoctorOnly")]
        public async Task<IActionResult> GetFinding(int appointmentId)
        {
            var finding = await _findingService.GetByAppointmentAsync(appointmentId);
            return finding == null ? NotFound() : Ok(finding);
        }

        // PUT api/doctor/appointments/15/finding
        [HttpPut("appointments/{appointmentId}/finding")]
        [Authorize(Policy = "DoctorOnly")]
        public async Task<IActionResult> SaveFinding(int appointmentId, [FromBody] UpsertFindingRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { message = "Invalid token." });

            var doctor = await _doctorService.GetByUserIdAsync(userId);

            if (doctor == null)
                return NotFound(new { message = "Doctor profile not found for this user." });

            try
            {
                return Ok(await _findingService.UpsertAsync(doctor.DoctorId, appointmentId, request));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}