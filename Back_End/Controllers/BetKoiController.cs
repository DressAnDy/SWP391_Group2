using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using KoiBet.DTO; // Ensure you have the correct namespace for DTOs
using Service.IBetKoiService;

namespace KoiBet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KoiBetController : ControllerBase
    {
        private readonly IBetKoiService _betKoiService;

        public KoiBetController(IBetKoiService betKoiService)
        {
            _betKoiService = betKoiService;
        }

        [HttpPost("Place Bet")]
        public async Task<IActionResult> PlaceBet([FromBody] CreateBetDTO createBetDto)
        {
            return await _betKoiService.HandlePlaceBet(createBetDto);
        }

        [HttpPost("Update Bet")]
        public async Task<IActionResult> UpdateBet([FromBody] UpdateBetDTO updateBetDto)
        {
            return await _betKoiService.HandleUpdateBet(updateBetDto);
        }

        [HttpDelete("Delete Bet")]
        public async Task<IActionResult> DeleteBet(string betId)
        {
            return await _betKoiService.HandleDeleteBet(betId);
        }

        [HttpGet("Get Bet")]
        public async Task<IActionResult> GetBet(string betId)
        {
            return await _betKoiService.HandleGetBet(betId);
        }

        [HttpGet("Get All Bet")]
        public async Task<IActionResult> GetAllBet()
        {
            return await _betKoiService.HandleGetAllBet();
        }


        [HttpGet("Get User Bets")]
        public async Task<IActionResult> GetUserBets(string userId)
        {
            return await _betKoiService.HandleGetUserBets(userId);
        }

        [HttpGet("Get Bet History By UserId")]
        public async Task<IActionResult> GetBetHistoryByUserId(string userId)
        {
            return await _betKoiService.HandleGetBetHistory(userId);
        }

        [HttpPost("Update Status Bet")]
        public async Task<IActionResult> UpdateStatusBet([FromBody] string betId, [FromBody] string newStatus)
        {
            return await _betKoiService.UpdateBetStatus(betId, newStatus);
        }
    }
}
