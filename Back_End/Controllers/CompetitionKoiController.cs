using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using KoiBet.DTO.Competition;
using Service.ICompetitionService;

namespace KoiBet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompetitionKoiController : ControllerBase
    {
        private readonly ICompetitionService _competitionService;

        public CompetitionKoiController(ICompetitionService competitionService)
        {
            _competitionService = competitionService;
        }

        [HttpGet("Get all CompetitionKoi")]
        public async Task<IActionResult> GetAllCompetition()
        {
            return await _competitionService.HandleGetAllCompetitions();
        }

        [HttpGet("Get competition")]
        public async Task<IActionResult> GetCompetition(string competitionId)
        {
            return await _competitionService.HandleGetCompetition(competitionId);
        }

        [HttpPost("Create CompetitionKoi")]
        public async Task<IActionResult> CreateCompetition([FromBody] CreateCompetitionDTO _createCompetition)
        {
            return await _competitionService.HandleCreateNewCompetition(_createCompetition);
        }

        [HttpPost("Update Competition")]
        public async Task<IActionResult> UpdateCompetition([FromBody] UpdateCompetitionDTO _updateCompetition)
        {
            return await _competitionService.HandleUpdateCompetition(_updateCompetition);

        }

        [HttpDelete("Delete Competition")]
        public async Task<IActionResult> DeleteCompetition(string competitionId)
        {
            return await _competitionService.HandleDeleteCompetition(competitionId);
        }
    }
}
