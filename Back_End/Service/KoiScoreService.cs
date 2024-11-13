using KoiBet.Data;
using KoiBet.DTO;
using KoiBet.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KoiBet.Service
{
    public interface IKoiScoreService
    {
        Task<IActionResult> HandleGetAllKoiScore();
        Task<IActionResult> HandleCreateKoiScore(string refereeId, CreateKoiScoreDTO createKoiScoreDTO);
        Task<IActionResult> HandleGetKoiScoreByRefereeId(string currentUserId, string competitionId);
        Task<IActionResult> HandleGetKoiScoreByKoiIdAndCompeId(SearchKoiScoreDTO searchKoiScoreDTO);
        //Task<IActionResult> HandleUpdateKoiScore(string refereeId, UpdateKoiScoreDTO updateKoiScoreDTO);
        //Task<IActionResult> HandleDeleteKoiScore(string koiScoreId);
        //Task<IActionResult> HandleGetKoiScoreById(string koiScoreId);
    }

    public class KoiScoreService : ControllerBase, IKoiScoreService
    {
        private readonly ApplicationDbContext _context;

        public KoiScoreService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all Matches
        public async Task<IActionResult> HandleGetAllKoiScore()
        {
            try
            {
                var scores = await _context.KoiScore
                    .Select(score => new KoiScore
                    {
                        score_id = score.score_id,
                        koi_id = score.koi_id,
                        referee_id = score.referee_id,
                        match_id = score.match_id,
                        score_koi = score.score_koi,
                        FishKoi = score.FishKoi,
                    })
                    .ToListAsync();

                if (!scores.Any())
                {
                    return NotFound("No matches found!");
                }

                return Ok(scores);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving matches: {ex.Message}");
            }
        }

        public async Task<IActionResult> HandleGetKoiScoreByRefereeId(string currentUserId, string competitionId)
        {
            try
            {
                var scores = _context.KoiScore
                    .Include(c => c.FishKoi)
                    .Include(c => c.Referee)
                    .Where(c => c.Referee.user_id == currentUserId)
                    .Where(c => c.match_id.Contains(competitionId))
                    .ToList();

                if (!scores.Any())
                {
                    return NotFound("No scores found!");
                }

                return Ok(scores);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving matches: {ex.Message}");
            }
        }

        public async Task<IActionResult> HandleGetKoiScoreByKoiIdAndCompeId(SearchKoiScoreDTO searchKoiScoreDTO)
        {
            try
            {
                var query = _context.KoiScore
                    .Include(c => c.FishKoi).ThenInclude(cs => cs.User)
                    .Include(c => c.Referee).ThenInclude(cs => cs.User)
                    .Where(c => c.koi_id == searchKoiScoreDTO.KoiId)
                    .AsQueryable();

                if(searchKoiScoreDTO.CompetitionId != null)
                {
                    query = query.Where(c => c.match_id.Contains(searchKoiScoreDTO.CompetitionId));
                }

                var scores = query.ToList();

                if (scores == null)
                {
                    return NotFound("No scores found!");
                }

                return Ok(scores);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving matches: {ex.Message}");
            }
        }

        // Create a new Match
        public async Task<IActionResult> HandleCreateKoiScore(string refereeId, CreateKoiScoreDTO createKoiScoreDTO)
        {
            try
            {
                var lastScore = await _context.KoiScore
                    .OrderByDescending(m => m.score_id)
                    .FirstOrDefaultAsync();

                var referee = await _context.Referee
                    .FirstOrDefaultAsync(c => c.user_id == refereeId);

                int newScoreNumber = 1;

                if (lastScore != null)
                {
                    var lastId = lastScore.score_id;
                    if (lastId.StartsWith("SCORE_"))
                    {
                        int.TryParse(lastId.Substring(6), out newScoreNumber);
                        newScoreNumber++;
                    }
                }

                var newScore = new KoiScore
                {
                    score_id = $"SCORE_{newScoreNumber}",
                    referee_id = referee.RefereeId,
                    score_koi = createKoiScoreDTO.Score,
                    koi_id = createKoiScoreDTO.KoiId,
                    match_id = createKoiScoreDTO.MatchId,
                    FishKoi = await _context.FishKoi.FirstOrDefaultAsync(c => c.koi_id == createKoiScoreDTO.KoiId),
                };

                _context.KoiScore.Add(newScore);



                var match = _context.CompetitionMatch
                    .FirstOrDefault(c => c.match_id == createKoiScoreDTO.MatchId);

                if(match.result == "Pending")
                {
                    match.result = newScore.FishKoi.koi_name + "_" + createKoiScoreDTO.Score;
                }
                else
                {
                    var result = decimal.Parse(match.result.Split('_')[1]);
                    if(result < createKoiScoreDTO.Score)
                    {
                        match.result = newScore.FishKoi.koi_name + "_" + createKoiScoreDTO.Score;
                    }
                    else if(result == createKoiScoreDTO.Score)
                    {
                        match.result = "Even_" + createKoiScoreDTO.Score;
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(newScore);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating score: {ex.Message}");
            }
        }

        //// Update an existing Match
        //public async Task<IActionResult> HandleUpdateKoiScore(string refereeId, UpdateKoiScoreDTO updateKoiScoreDTO)
        //{
        //    try
        //    {
        //        var match = await _context.CompetitionMatch
        //            .FirstOrDefaultAsync(m => m.match_id == matchId);

        //        if (match == null)
        //        {
        //            return NotFound("Match not found!");
        //        }

        //        match.first_koiId1 = updateMatchDto.FishkoiId_1;
        //        match.round_id = updateMatchDto.RoundId;
        //        match.first_koiId2 = updateMatchDto.FishkoiId_2;
        //        match.result = updateMatchDto.Result;

        //        _context.CompetitionMatch.Update(match);
        //        var result = await _context.SaveChangesAsync();

        //        if (result != 1)
        //        {
        //            return BadRequest("Failed to update match!");
        //        }

        //        return Ok(match);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest($"Error updating match: {ex.Message}");
        //    }
        //}

        //// Delete a Match
        //public async Task<IActionResult> HandleDeleteMatch(string matchId)
        //{
        //    try
        //    {
        //        var match = await _context.CompetitionMatch
        //            .FirstOrDefaultAsync(m => m.match_id == matchId);

        //        if (match == null)
        //        {
        //            return NotFound("Match not found!");
        //        }

        //        _context.CompetitionMatch.Remove(match);
        //        var result = await _context.SaveChangesAsync();

        //        if (result != 1)
        //        {
        //            return BadRequest("Failed to delete match!");
        //        }

        //        return Ok("Match deleted successfully!");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest($"Error deleting match: {ex.Message}");
        //    }
        //}

        //// Get a specific Match by ID
        //public async Task<IActionResult> HandleGetMatch(string matchId)
        //{
        //    try
        //    {
        //        var match = await _context.CompetitionMatch
        //            .Select(m => new CompetitionMatchDTO
        //            {
        //                MatchId = m.match_id,
        //                FishkoiId_1 = m.first_koiId1,
        //                RoundId = m.round_id,
        //                FishkoiId_2 = m.first_koiId2,
        //                Result = m.result
        //            })
        //            .FirstOrDefaultAsync(m => m.MatchId == matchId);

        //        if (match == null)
        //        {
        //            return NotFound("Match not found!");
        //        }

        //        return Ok(match);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest($"Error retrieving match: {ex.Message}");
        //    }
        //}
    }
}
