using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using KoiBet.DTO.Award; // Ensure you have the correct namespace for DTOs
using KoiBet.Service;

namespace KoiBet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AwardController : ControllerBase
    {
        private readonly IAwardService _awardService;

        public AwardController(IAwardService awardService)
        {
            _awardService = awardService;
        }

        [HttpPost("Create Award")]
        public async Task<IActionResult> CreateAward([FromBody] CreateAwardDTO createAwardDto)
        {
            return await _awardService.HandleCreateAward(createAwardDto);
        }

        [HttpPut("Update Award")]
        public async Task<IActionResult> UpdateAward([FromBody] UpdateAwardDTO updateAwardDto)
        {
            return await _awardService.HandleUpdateAward(updateAwardDto.award_id, updateAwardDto);
        }

        [HttpDelete("Delete Award")]
        public async Task<IActionResult> DeleteAward(string awardId)
        {
            return await _awardService.HandleDeleteAward(awardId);
        }

        [HttpGet("Get Award")]
        public async Task<IActionResult> GetAward(string awardId)
        {
            return await _awardService.HandleGetAwardById(awardId);
        }

        //[HttpGet("Get All Awards")]
        //public async Task<IActionResult> GetAllAwards()
        //{
        //    return await _awardService.HandleGetAllAwards();
        //}

        [HttpGet("Get All Award")]
        public async Task<IActionResult> GetAllAward()
        {
            return await _awardService.HandleGetAllAwardsWithCompetitions();
        }
    }
}
