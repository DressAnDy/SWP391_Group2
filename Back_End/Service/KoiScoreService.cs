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
        Task<IActionResult> HandleGetKoiScoreByRefereeId(string currentUserId);
        Task<IActionResult> HandleGetKoiScoreByKoiIdAndCompeId(SearchKoiScoreDTO searchKoiScoreDTO);
        Task<IActionResult> HandleUpdateKoiScore(string refereeId, UpdateKoiScoreDTO updateKoiScoreDTO);
        //Task<IActionResult> HandleDeleteKoiScore(string koiScoreId);
        Task<IActionResult> HandleGetKoiScoreByUserId(string userId);
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
                    return NotFound("No scores found!");
                }

                return Ok(scores);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving scores: {ex.Message}");
            }
        }

        public async Task<IActionResult> HandleGetKoiScoreByRefereeId(string currentUserId)
        {
            try
            {
                var scores = _context.KoiScore
                    .Include(c => c.FishKoi)
                    .Include(c => c.Referee)
                    .Where(c => c.Referee.user_id == currentUserId)
                    .ToList();

                if (!scores.Any())
                {
                    return NotFound("No scores found!");
                }

                return Ok(scores);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving scores: {ex.Message}");
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
                return BadRequest($"Error retrieving scores: {ex.Message}");
            }
        }

        // Create a new Match
        public async Task<IActionResult> HandleCreateKoiScore(string refereeId, CreateKoiScoreDTO createKoiScoreDTO)
        {
            try
            {
                var matchQuery = _context.CompetitionMatch
                    .AsQueryable();

                var referee = await _context.Referee
                    .FirstOrDefaultAsync(c => c.user_id == refereeId);

                var compe = await _context.CompetitionKoi
                    .FirstOrDefaultAsync(c => createKoiScoreDTO.MatchId.Contains(c.competition_id));

                if(compe != null && compe.referee_id != referee.RefereeId)
                {
                    return BadRequest("Referee not authorized!");
                }
                else if(compe.status_competition == "Finished")
                {
                    return BadRequest("Competition is finished!");
                }

                var scoreExisted = _context.KoiScore
                    .FirstOrDefault(c => c.koi_id == createKoiScoreDTO.KoiId && c.match_id == createKoiScoreDTO.MatchId);

                if(scoreExisted != null)
                {
                    scoreExisted.score_koi = createKoiScoreDTO.Score;
                    _context.Update(scoreExisted);
                    _context.SaveChanges();
                    return Ok("Score existed and updated!");
                }

                var roundValidateArray = createKoiScoreDTO.MatchId.Split('_');

                var matchValidate = matchQuery
                    .FirstOrDefault(c => c.match_id.Contains(roundValidateArray[0] + "_" + roundValidateArray[1] + "_" + roundValidateArray[2] + "_" + (int.Parse(roundValidateArray[3]) + 1)));

                if(matchValidate != null)
                {
                    return BadRequest("Match already finished!");
                }

                var lastScore = await _context.KoiScore
                    .OrderByDescending(m => m.score_id)
                    .FirstOrDefaultAsync();

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



                var match = matchQuery
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
                        return BadRequest("Score is even!");
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

        // Update an existing Match
        public async Task<IActionResult> HandleUpdateKoiScore(string refereeId, UpdateKoiScoreDTO updateKoiScoreDTO)
        {
            try
            {
                var compe = await _context.CompetitionKoi
                    .FirstOrDefaultAsync(c => updateKoiScoreDTO.MatchId.Contains(c.competition_id));

                if (compe == null)
                {
                    return NotFound("Competition not found!");
                }
                else if (compe.status_competition == "Finished")
                {
                    return BadRequest("Competition is finished!");
                }

                var match = await _context.CompetitionMatch
                    .FirstOrDefaultAsync(m => m.match_id == updateKoiScoreDTO.MatchId);

                if (match == null)
                {
                    return NotFound("Match not found!");
                }

                var score = _context.KoiScore
                    .FirstOrDefault(c => c.score_id == updateKoiScoreDTO.ScoreId);

                if (score == null)
                {
                    return BadRequest("Score not found!");
                }

                score.score_koi = updateKoiScoreDTO.Score;
                _context.KoiScore.Update(score);

                var fishKoi = _context.FishKoi
                    .FirstOrDefault(c => c.koi_id == updateKoiScoreDTO.KoiId);

                var result = decimal.Parse(match.result.Split('_')[1]);
                if (result < updateKoiScoreDTO.Score)
                {
                    match.result = fishKoi.koi_name + "_" + updateKoiScoreDTO.Score;
                }
                else if (result == updateKoiScoreDTO.Score)
                {
                    return BadRequest("Score is even!");
                }

                _context.CompetitionMatch.Update(match);

                _context.SaveChanges();

                return Ok(match);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating match: {ex.Message}");
            }
        }

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

        // Get a specific Match by ID
        public async Task<IActionResult> HandleGetKoiScoreByUserId(string userId)
        {
            try
            {
                var koiScores = _context.KoiScore
                    .AsQueryable();

                var koiFishList = _context.FishKoi
                    .Where(c => c.users_id == userId)
                    .ToList();

                List<KoiScore> scores = new List<KoiScore>();

                foreach( var koi in koiFishList )
                {
                    scores.AddRange(koiScores
                        .Include(c => c.FishKoi).ThenInclude(cs => cs.User)
                        .Include(c => c.Referee)
                        .Where(c => c.koi_id == koi.koi_id)
                        .ToList());
                }

                return Ok(scores);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving match: {ex.Message}");
            }
        }
    }
}
