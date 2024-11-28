using KoiBet.DTO.CompetitionRound;
using KoiBet.Service;
using Microsoft.AspNetCore.Mvc;

namespace KoiBet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompetitionRoundController : ControllerBase
    {
        private readonly ICompetitionRoundService _competitionRound;

        public CompetitionRoundController(ICompetitionRoundService competitionRoundService)
        {
            _competitionRound = competitionRoundService;
        }

        [HttpGet("Get All CompetitionRound")]
        public async Task<IActionResult> GetAllCompetitionRound()
        {
            return await _competitionRound.HandleGetAllRounds();
        }

        [HttpGet("Get CompetitionRound By Id")]
        public async Task<IActionResult> GetCompetitionRoundById(string roundId)
        {
            return await _competitionRound.HandleGetRound(roundId);
        }

        [HttpPost("Create CompetitionRound")]
        public async Task<IActionResult> CreateComepetitionRound([FromBody] CreateCompetitionRoundDTO _createCompetitionRound)
        {
            return await _competitionRound.HandleCreateNewRound(_createCompetitionRound);
        }

        [HttpGet("Update CompetitionRound")]
        public async Task<IActionResult> UpdateCompetitionRound( string roundId,
            [FromBody] UpdateCompetitionRoundDTO _updateCompetitionRound)
        {
            return await _competitionRound.HandleUpdateRound(roundId, _updateCompetitionRound);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCompetitionRound(string roundId)
        {
            return await _competitionRound.HandleDeleteRound(roundId);
        }
    }    
}