using KoiBet.Data;
using KoiBet.DTO.CompetitionMatch;
using KoiBet.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KoiBet.Service
{
    public interface ICompetitionMatchService
    {
        Task<IActionResult> HandleGetAllMatches();
        Task<IActionResult> HandleCreateNewMatch(CreateCompetitionMatchDTO createMatchDto);
        Task<IActionResult> HandleUpdateMatch(string matchId, UpdateCompetitionMatchDTO updateMatchDto);
        Task<IActionResult> HandleDeleteMatch(string matchId);
        Task<IActionResult> HandleGetMatch(string matchId);
    }

    public class CompetitionMatchService : ControllerBase, ICompetitionMatchService
    {
        private readonly ApplicationDbContext _context;

        public CompetitionMatchService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all Matches
        public async Task<IActionResult> HandleGetAllMatches()
        {
            try
            {
                var matches = await _context.CompetitionMatch
                    .Select(match => new CompetitionMatchDTO
                    {
                        MatchId = match.match_id,
                        FirstKoi = match.first_koi,
                        RoundId = match.round_id,
                        SecondKoi = match.second_koi,
                        Result = match.result
                    })
                    .ToListAsync();

                if (!matches.Any())
                {
                    return NotFound("No matches found!");
                }

                return Ok(matches);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving matches: {ex.Message}");
            }
        }

        // Create a new Match
        public async Task<IActionResult> HandleCreateNewMatch(CreateCompetitionMatchDTO createMatchDto)
        {
            try
            {
                var lastMatch = await _context.CompetitionMatch
                    .OrderByDescending(m => m.match_id)
                    .FirstOrDefaultAsync();

                int newMatchNumber = 1; 

                if (lastMatch != null)
                {
                    var lastId = lastMatch.match_id;
                    if (lastId.StartsWith("Match_"))
                    {
                        int.TryParse(lastId.Substring(6), out newMatchNumber);
                        newMatchNumber++;
                    }
                }

                var newMatch = new CompetitionMatch
                {
                    match_id = $"Match_{newMatchNumber}",
                    first_koi = createMatchDto.FirstKoi,
                    round_id = createMatchDto.RoundId,
                    second_koi = createMatchDto.SecondKoi,
                    result = createMatchDto.Result
                };

                _context.CompetitionMatch.Add(newMatch);
                var result = await _context.SaveChangesAsync();

                if (result != 1)
                {
                    return BadRequest("Failed to create new match!");
                }

                return Ok(newMatch);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating match: {ex.Message}");
            }
        }

        // Update an existing Match
        public async Task<IActionResult> HandleUpdateMatch(string matchId, UpdateCompetitionMatchDTO updateMatchDto)
        {
            try
            {
                var match = await _context.CompetitionMatch
                    .FirstOrDefaultAsync(m => m.match_id == matchId);

                if (match == null)
                {
                    return NotFound("Match not found!");
                }

                match.first_koi = updateMatchDto.FirstKoi;
                match.round_id = updateMatchDto.RoundId;
                match.second_koi = updateMatchDto.SecondKoi;
                match.result = updateMatchDto.Result;

                _context.CompetitionMatch.Update(match);
                var result = await _context.SaveChangesAsync();

                if (result != 1)
                {
                    return BadRequest("Failed to update match!");
                }

                return Ok(match);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating match: {ex.Message}");
            }
        }

        // Delete a Match
        public async Task<IActionResult> HandleDeleteMatch(string matchId)
        {
            try
            {
                var match = await _context.CompetitionMatch
                    .FirstOrDefaultAsync(m => m.match_id == matchId);

                if (match == null)
                {
                    return NotFound("Match not found!");
                }

                _context.CompetitionMatch.Remove(match);
                var result = await _context.SaveChangesAsync();

                if (result != 1)
                {
                    return BadRequest("Failed to delete match!");
                }

                return Ok("Match deleted successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting match: {ex.Message}");
            }
        }

        // Get a specific Match by ID
        public async Task<IActionResult> HandleGetMatch(string matchId)
        {
            try
            {
                var match = await _context.CompetitionMatch
                    .Select(m => new CompetitionMatchDTO
                    {
                        MatchId = m.match_id,
                        FirstKoi = m.first_koi,
                        RoundId = m.round_id,
                        SecondKoi = m.second_koi,
                        Result = m.result
                    })
                    .FirstOrDefaultAsync(m => m.MatchId == matchId);

                if (match == null)
                {
                    return NotFound("Match not found!");
                }

                return Ok(match);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving match: {ex.Message}");
            }
        }
    }
}
