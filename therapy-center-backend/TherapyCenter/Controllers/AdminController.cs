using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TherapyCenter.DTO_s.Admin;
using TherapyCenter.Services.Interfaces;


namespace TherapyCenter.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // ── Therapies ──────────────────────────────────────────────────────────

        // GET api/admin/therapies
        [HttpGet("therapies")]
        public async Task<IActionResult> GetTherapies()
            => Ok(await _adminService.GetAllTherapiesAsync());

        // POST api/admin/therapies
        [HttpPost("therapies")]
        public async Task<IActionResult> CreateTherapy([FromBody] CreateTherapyRequest request)
        {
            var therapy = await _adminService.CreateTherapyAsync(request);
            return CreatedAtAction(nameof(GetTherapies), new { id = therapy.TherapyId }, therapy);
        }

        // PUT api/admin/therapies/3
        [HttpPut("therapies/{id}")]
        public async Task<IActionResult> UpdateTherapy(int id, [FromBody] UpdateTherapyRequest request)
        {
            try
            {
                return Ok(await _adminService.UpdateTherapyAsync(id, request));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // DELETE api/admin/therapies/3
        [HttpDelete("therapies/{id}")]
        public async Task<IActionResult> DeleteTherapy(int id)
        {
            await _adminService.DeleteTherapyAsync(id);
            return NoContent();
        }

        // ── Doctor profile ─────────────────────────────────────────────────────

        // POST api/admin/doctors/profile
        [HttpPost("doctors/profile")]
        public async Task<IActionResult> CreateDoctorProfile([FromBody] CreateDoctorProfileRequest request)
        {
            try
            {
                return Ok(await _adminService.CreateDoctorProfileAsync(request));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        // DELETE api/admin/doctors/{id}
        [HttpDelete("doctors/{id}")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            try
            {
                await _adminService.DeleteDoctorAsync(id);

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // ── Receptionists ──────────────────────────────────────────────────────

        // GET api/admin/receptionists
        [HttpGet("receptionists")]
        public async Task<IActionResult> GetReceptionists()
            => Ok(await _adminService.GetAllReceptionistsAsync());
        // DELETE api/admin/staff/7
        [HttpDelete("staff/{id}")]
        public async Task<IActionResult> DeleteStaff(int id)
        {
            try
            {
                await _adminService.DeactivateStaffAsync(id);
                return NoContent();
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

        // ── Slot generation ────────────────────────────────────────────────────

        // POST api/admin/slots/generate
        [HttpPost("slots/generate")]
        public async Task<IActionResult> GenerateSlots([FromBody] GenerateSlotsRequest request)
        {
            try
            {
                var count = await _adminService.GenerateSlotsForDoctorAsync(request);
                return Ok(new { message = $"{count} slots generated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
