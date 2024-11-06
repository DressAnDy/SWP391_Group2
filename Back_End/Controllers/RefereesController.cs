using KoiBet.DTO;
using KoiBet.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KoiBet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RefereesController : ControllerBase
    {
        private readonly IRefereeService _refereeService;

        public RefereesController(IRefereeService refereeService)
        {
            _refereeService = refereeService;
        }

        // Get all referees
        [Authorize]
        [HttpPost("GetAllReferees")]
        public async Task<IActionResult> GetAllReferees()
        {
            var currentUser = HttpContext.User;
            var currentUserRole = currentUser.FindFirst(ClaimTypes.Role)?.Value;

            // Ki?m tra quy?n truy c?p
            if (currentUserRole != "admin" && currentUserRole != "manager")
            {
                return BadRequest(new { message = "Unauthorized!" });
            }

            return await _refereeService.HandleGetAllReferees();
        }

        // Get a specific referee by ID
        [Authorize]
        [HttpPost("GetReferee")]
        public async Task<IActionResult> GetReferee([FromBody] string refereeId)
        {
            var currentUser = HttpContext.User;
            var currentUserRole = currentUser.FindFirst(ClaimTypes.Role)?.Value;

            // Ki?m tra quy?n truy c?p
            if (currentUserRole != "admin" && currentUserRole != "manager")
            {
                return BadRequest(new { message = "Unauthorized!" });
            }

            return await _refereeService.HandleGetReferee(refereeId);
        }

        // Create a new referee
        [HttpPost("CreateReferee")]
        public async Task<IActionResult> CreateReferee([FromBody] CreateRefereeDTO createRefereeDto)
        {
            return await _refereeService.HandleCreateNewReferee(createRefereeDto);
        }

        // Update an existing referee
        [HttpPost("UpdateReferee")]
        public async Task<IActionResult> UpdateReferee([FromBody] UpdateRefereeDTO updateRefereeDto)
        {
            return await _refereeService.HandleUpdateReferee(updateRefereeDto.RefereeId, updateRefereeDto);
        }

        // Delete a referee
        [HttpDelete("DeleteReferee")]
        public async Task<IActionResult> DeleteReferee(string refereeId)
        {
            return await _refereeService.HandleDeleteReferee(refereeId);
        }
    }
}
