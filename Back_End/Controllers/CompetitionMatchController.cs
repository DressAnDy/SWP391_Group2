using KoiBet.DTO.CompetitionMatch;
using KoiBet.Service;
using Microsoft.AspNetCore.Mvc;

namespace KoiBet.Controllers;

    [Route("api/[controller]")]
    [ApiController]
public class CompetitionMatchController : ControllerBase
{
    private readonly ICompetitionMatchService _competitionMatch;

    public CompetitionMatchController(ICompetitionMatchService competitionMatchService)
    {
        _competitionMatch = competitionMatchService;
    }

    [HttpGet("Get All CompetitionMatch")]
    public async Task<IActionResult> GettAllCompetitionMatch()
    {
        return await _competitionMatch.HandleGetAllMatches();
    }

    [HttpGet("Get Competition By Id")]
    public async Task<IActionResult> GetCompetitionMatchById(string competitionMatchId)
    {
        return await _competitionMatch.HandleGetMatch(competitionMatchId);
    }

    [HttpPost("Create CompetitionMatch")]
    public async Task<IActionResult> CreateCompetitionMatch([FromBody]CreateCompetitionMatchDTO _createCompetitionMatch)
    {
        return await _competitionMatch.HandleCreateNewMatch(_createCompetitionMatch);
    }

    [HttpGet("Update CompetitionMatch")]
    public async Task<IActionResult> UpdateCompetitionMatch(string matchId, [FromBody] UpdateCompetitionMatchDTO _updateCompetitionMatch)
    {
        return await _competitionMatch.HandleUpdateMatch(matchId, _updateCompetitionMatch);
    }

    [HttpDelete("Delete CompetitionMatch")]
    public async Task<IActionResult> DeleteCompetitionMatch(string matchId)
    {
        return await _competitionMatch.HandleDeleteMatch(matchId);
    }
}