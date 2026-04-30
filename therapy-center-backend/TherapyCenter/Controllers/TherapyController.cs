using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TherapyController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public TherapyController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // GET api/therapy
        // Any authenticated user can load therapy options for booking.
        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _adminService.GetAllTherapiesAsync());
    }
}
