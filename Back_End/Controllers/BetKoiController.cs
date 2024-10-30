using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using KoiBet.DTO.Bet; // Ensure you have the correct namespace for DTOs
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

        [HttpGet("Get User Bets")]
        public async Task<IActionResult> GetUserBets(string userId)
        {
            return await _betKoiService.HandleGetUserBets(userId);
        }
    }
}
