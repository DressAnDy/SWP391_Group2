using KoiBet.Data;
using DTO;
using KoiBet.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Service.KoiFishService;
using System.Security.Claims;

namespace KoiBet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KoiFishController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IKoiFishService _koiFishService;

        public KoiFishController(ApplicationDbContext context, IKoiFishService koiFishService)
        {
            _context = context;
            _koiFishService = koiFishService;
        }

        [Authorize]
        [HttpPost("Get All Koi Fishes")]
        public async Task<IActionResult> GetAllKoiFishes()
        {
            var currentUser = HttpContext.User;
            var currentUserRole = currentUser.FindFirst(ClaimTypes.Role)?.Value;

            // Ki?m tra quy?n truy c?p
            if (currentUserRole != "admin" && currentUserRole != "manager")
            {
                return BadRequest(new { message = "Unauthorized!" });
            }

            return await _koiFishService.HandleGetAllKoiFishes();
        }


        [HttpPost("Get Koi Fish By Id")]
        public async Task<IActionResult> GetKoiFishById([FromBody] SearchKoiDTO searchKoiDTO)
        {
            return await _koiFishService.HandleGetKoiFishById(searchKoiDTO);
        }


        [HttpPost("Get Koi Fish By User Id")]
        public async Task<IActionResult> GetKoiFishByUserId([FromBody] SearchKoiUserIdDTO searchKoiUserId)
        {
            return await _koiFishService.HandleGetKoiFishByUserId(searchKoiUserId);
        }

        [HttpPost("Create New Koi Fish")]
        public async Task<IActionResult> CreateNewKoiFish([FromBody] CreateKoiFishDTO createKoiFishDto)
        {
            return await _koiFishService.HandleCreateNewKoiFish(createKoiFishDto);
        }

        [HttpPost("Update Koi Fish")]
        public async Task<IActionResult> UpdateKoiFish([FromBody] UpdateKoiFishDTO updateKoiFishDto)
        {
            return await _koiFishService.HandleUpdateKoiFish(updateKoiFishDto);
        }

        [HttpPost("Delete Koi Fish")]
        public async Task<IActionResult> DeleteKoiFish([FromBody] SearchKoiDTO searchKoiDTO)
        {
            return await _koiFishService.HandleDeleteKoiFish(searchKoiDTO.koi_id);
        }
    }
}
