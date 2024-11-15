using KoiBet.Data;
using KoiBet.DTO;
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
        Task<IActionResult> HandleProcessingMatch(ProcessingMatchDTO ProcessingMatchDto);
        Task<IActionResult> HandleDeleteMatch(string matchId);
        Task<IActionResult> HandleGetMatch(string matchId);
        Task<IActionResult> HandleGetMatchByCompeId(string competitionMatchId);
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
                    .Include(c => c.FirstKoi).ThenInclude(cb => cb.User)
                    .Include(c => c.SecondKoi).ThenInclude(cb => cb.User)
                    .Include(c => c.Scores).ThenInclude(cb => cb.Referee).ThenInclude(cd=> cd.User)
                    //.Select(match => new CompetitionMatch
                    //{
                    //    match_id = match.match_id,
                    //    first_koiId1 = match.first_koiId1,
                    //    first_koiId2 = match.first_koiId2,
                    //    round_id = match.round_id,
                    //    result = match.result,
                    //    FirstKoi = match.FirstKoi,
                    //    SecondKoi = match.SecondKoi,
                    //})
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
                    first_koiId1 = createMatchDto.FishkoiId_1,
                    round_id = createMatchDto.RoundId,
                    first_koiId2 = createMatchDto.FishkoiId_2,
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

                match.first_koiId1 = updateMatchDto.FishkoiId_1;
                match.round_id = updateMatchDto.RoundId;
                match.first_koiId2 = updateMatchDto.FishkoiId_2;
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

        // Update an existing Match result
        public async Task<IActionResult> HandleProcessingMatch(ProcessingMatchDTO processingMatchDto)
        {
            try
            {
                var compeQuery = _context.CompetitionKoi
                    .AsQueryable();

                var currCompe = compeQuery
                    .FirstOrDefault(c => c.competition_id == processingMatchDto.CompetitionId);

                var matchQuery = _context.CompetitionMatch
                    .OrderByDescending(m => m.match_id)
                    .AsQueryable();

                var resultValidate = matchQuery
                    .FirstOrDefault(c => c.result.Contains("Pending") && c.match_id.Contains(currCompe.competition_id));

                if (resultValidate != null)
                {
                    return BadRequest("Round not finished!");
                }

                var matchList = matchQuery
                    .OrderByDescending(m => m.match_id)
                    .Include(c => c.Round)
                    .Where(m => m.round_id.Contains(currCompe.competition_id))
                    .ToList();

                List<string> id = matchList[0].match_id.Split('_').ToList();

                if (id[3] == currCompe.rounds)
                {
                    var winnerId = matchList[0].result.Split('_')[0];
                    currCompe.koi_id = winnerId;
                    _context.CompetitionKoi.Update(currCompe);
                    _context.SaveChanges();
                    return Ok("Competition finished and the winner is " + winnerId + "!");
                }

                var currRound = int.Parse(id[3]);

                var newId = id[0] + "_" + id[1] + "_" + id[2] + "_" + (currRound + 1);

                var koiList = new List<string>();

                for(int i = 0; i < matchList.Count; i++)
                {
                    var processKoi = matchList[i].result.Split('_')[0];
                    koiList.Add(processKoi);
                }

                for(int i = 0; i < koiList.Count; i++)
                {
                    if(i % 2 == 0)
                    {
                        var processMatch = new CompetitionMatch
                        {
                            round_id = newId,
                            first_koiId1 = koiList[i],
                            first_koiId2 = koiList[i + 1],
                            match_id = newId + "_MATCH_" + (i + 1),  
                            result = "Pending"
                        };

                        _context.CompetitionMatch.Add(processMatch);
                    }
                }
                
                await _context.SaveChangesAsync();

                return Ok("successful");
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
                    .Include(c => c.FirstKoi).ThenInclude(cb => cb.User)
                    .Include(c => c.SecondKoi).ThenInclude(cb => cb.User)
                    .Include(c => c.Scores).ThenInclude(cb => cb.Referee).ThenInclude(cd => cd.User)
                    .FirstOrDefaultAsync(m => m.match_id == matchId);

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

        // Get a specific Match by CompeID
        public async Task<IActionResult> HandleGetMatchByCompeId(string compeId)
        {
            try
            {
                var matches = _context.CompetitionMatch
                    .Include(c => c.FirstKoi).ThenInclude(cb => cb.User)
                    .Include(c => c.SecondKoi).ThenInclude(cb => cb.User)
                    .Include(c => c.Scores).ThenInclude(cb => cb.Referee).ThenInclude(cd => cd.User)
                    .Where(m => m.match_id.Contains(compeId))
                    .ToList();

                if (matches == null)
                {
                    return NotFound("Match not found!");
                }

                return Ok(matches);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving match: {ex.Message}");
            }
        }
    }
}
