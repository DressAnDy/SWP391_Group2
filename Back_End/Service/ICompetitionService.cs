using Microsoft.AspNetCore.Mvc;
using KoiBet.Data;
using KoiBet.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using KoiBet.DTO.Competition;
using KoiBet.Service;
using KoiBet.DTO;
using System.ComponentModel.Design;
using Service.KoiFishService;
using DTO;

namespace Service.ICompetitionService
{
    public interface ICompetitionService
    {
        Task<IActionResult> HandleGetAllCompetitions();
        Task<IActionResult> HandleCreateNewCompetition(CreateCompetitionDTO createCompetitionDto);
        Task<IActionResult> HandleUpdateCompetition(UpdateCompetitionDTO updateCompetitionDto);
        Task<IActionResult> HandleDeleteCompetition(string competitionId);
        Task<IActionResult> HandleGetCompetition(string competitionId);
    }

    public class CompetitionService : ControllerBase, ICompetitionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IKoiCategoryService _koiCategoryService;
        private readonly IKoiFishService _koiService;
        private readonly IRefereeService _refereeService;
        private readonly IAwardService _awardService;




        public CompetitionService(ApplicationDbContext context, IKoiFishService koiService, IRefereeService refereeService, IAwardService awardService, IKoiCategoryService koiCategoryService)
        {
            _context = context;
            _koiService = koiService;
            _refereeService = refereeService;
            _awardService = awardService;
            _koiCategoryService = koiCategoryService;
        }

        // Get all Competitions
        public async Task<IActionResult> HandleGetAllCompetitions()
        {
            try
            {
                var competitions = await _context.CompetitionKoi
                    .Include(c => c.Award)
                    .Include(c => c.Referee).ThenInclude(cs => cs.User)
                    .Include(c => c.Category).ThenInclude(cs => cs.KoiStandard)
                    .Include(c => c.KoiRegistrations).ThenInclude(cs => cs.FishKoi)
                    .Include(c => c.Bets)
                    .ToListAsync();

                if (!competitions.Any())
                {
                    return BadRequest("No Competitions!");
                }

                return Ok(competitions);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving competitions: {ex.Message}");
            }
        }


        // Create a new Competition
        public async Task<IActionResult> HandleCreateNewCompetition(CreateCompetitionDTO createCompetitionDto)
        {
            try
            {
                var competitionQuery = _context.CompetitionKoi
                    .AsQueryable();

                var refereeValidate = competitionQuery
                    .FirstOrDefault(c => c.referee_id == createCompetitionDto.referee_id && c.status_competition == "Active");

                if(refereeValidate != null)
                {
                    return BadRequest("Referee not available!");
                }

                var lastCompetition = competitionQuery
                    .OrderByDescending(c => c.competition_id)
                    .ToList();

                int newIdNumber = 1; // Default ID starts at 1 if no records exist

                if (lastCompetition.Count != 0)
                {
                    var lastId = lastCompetition[0].competition_id;
                    if (lastId.StartsWith("CPT_"))
                    {
                        int.TryParse(lastId.Substring(4), out newIdNumber); // Increment the ID number after 'CPT_'
                        newIdNumber++;
                    }
                }

                var newCompetition = new CompetitionKoi
                {
                    competition_id = $"CPT_{newIdNumber}",
                    competition_name = createCompetitionDto.competition_name,
                    competition_description = createCompetitionDto.competition_description,
                    start_time = createCompetitionDto.start_time,
                    end_time = createCompetitionDto.end_time,
                    status_competition = createCompetitionDto.status_competition,
                    rounds = (Math.Log(createCompetitionDto.number_attendees, 2)).ToString(),
                    category_id = createCompetitionDto.category_id,
                    koi_id = "Pending",
                    referee_id = createCompetitionDto.referee_id,
                    award_id = createCompetitionDto.award_id,
                    competition_img = createCompetitionDto.competition_img,
                    number_attendees = createCompetitionDto.number_attendees,
                    //number_attendees = 2^(int.Parse(createCompetitionDto.rounds)),
                };

                _context.CompetitionKoi.Add(newCompetition);
                var result = await _context.SaveChangesAsync();

                if (result != 1)
                {
                    return BadRequest("Failed to create new competition!");
                }

                return Ok(newCompetition);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating competition: {ex.Message}");
            }
        }

        // Update an existing Competition
        public async Task<IActionResult> HandleUpdateCompetition(UpdateCompetitionDTO updateCompetitionDto)
        {
            try
            {
                var competitionQuery = _context.CompetitionKoi
                    .OrderByDescending(c => c.competition_id)
                    .AsQueryable();

                var refereeValidate = competitionQuery
                    .FirstOrDefault(c => c.referee_id == updateCompetitionDto.RefereeId && c.status_competition == "Active" && c.competition_id != updateCompetitionDto.CompetitionId);

                if (refereeValidate != null)
                {
                    return BadRequest("Referee not available!");
                }

                var competition = competitionQuery
                    .Include(c => c.KoiRegistrations)
                    .FirstOrDefault(c => c.competition_id == updateCompetitionDto.CompetitionId);

                if (competition == null)
                {
                    return NotFound("Competition not found!");
                }
                else if(competition.number_attendees != updateCompetitionDto.number_attendees || updateCompetitionDto.StatusCompetition == "Inactive")
                {
                    if(competition.KoiRegistrations.Count != 0)
                    {
                        return BadRequest("Competition is started. Can't update!");
                    }
                }

                var roundQuery = _context.CompetitionRound
                    .Where(c => c.competition_id == updateCompetitionDto.CompetitionId)
                    .AsQueryable();

                var oldRoundList = roundQuery
                    .OrderBy(c => c.RoundId)
                    .ToList();

                var roundCount = (Math.Log(updateCompetitionDto.number_attendees, 2)).ToString();

                if (oldRoundList.Count == 0 && updateCompetitionDto.StatusCompetition == "Active")
                {
                    for(int i = 0; i < int.Parse(roundCount); i++)
                    {
                        var count = (int.Parse(roundCount) - (i + 1)).ToString();
                            
                        var round = new CompetitionRound
                        {
                            RoundId = updateCompetitionDto.CompetitionId + "_ROUND_" + (i + 1),
                            competition_id = updateCompetitionDto.CompetitionId,
                            Match = (int)Math.Pow(2, Double.Parse(count)),
                        };

                        _context.CompetitionRound.Add(round);
                    }
                }
                else if(oldRoundList.Count != 0 && updateCompetitionDto.StatusCompetition == "Active")
                {
                    if(competition.number_attendees != updateCompetitionDto.number_attendees)
                    {
                        _context.CompetitionRound.RemoveRange(oldRoundList);

                        for (int i = 0; i < int.Parse(roundCount); i++)
                        {
                            var count = (int.Parse(roundCount) - (i + 1)).ToString();

                            var round = new CompetitionRound
                            {
                                RoundId = updateCompetitionDto.CompetitionId + "_ROUND_" + (i + 1),
                                competition_id = updateCompetitionDto.CompetitionId,
                                Match = (int)Math.Pow(2, Double.Parse(count)),
                            };

                            _context.CompetitionRound.Add(round);
                        }
                    }
                }
                else if(updateCompetitionDto.StatusCompetition == "Inactive")
                {
                    _context.CompetitionRound.RemoveRange(oldRoundList);
                }

                competition.competition_name = updateCompetitionDto.CompetitionName;
                competition.competition_description = updateCompetitionDto.CompetitionDescription;
                competition.status_competition = updateCompetitionDto.StatusCompetition;
                competition.category_id = updateCompetitionDto.KoiCategoryId;
                competition.referee_id = updateCompetitionDto.RefereeId;
                competition.award_id = updateCompetitionDto.AwardId;
                competition.competition_img = updateCompetitionDto.CompetitionImg;
                competition.number_attendees = updateCompetitionDto.number_attendees;
                competition.rounds = roundCount;

                _context.CompetitionKoi.Update(competition);

                _context.SaveChanges();

                return Ok(competition);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating competition: {ex.Message}");
            }
        }

        // Delete a Competition
        public async Task<IActionResult> HandleDeleteCompetition(string competitionId)
        {
            try
            {
                var competition = await _context.CompetitionKoi
                    .FirstOrDefaultAsync(c => c.competition_id == competitionId);

                if (competition == null)
                {
                    return NotFound("Competition not found!");
                }

                _context.CompetitionKoi.Remove(competition);
                var result = await _context.SaveChangesAsync();

                if (result != 1)
                {
                    return BadRequest("Failed to delete competition!");
                }

                return Ok("Competition deleted successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting competition: {ex.Message}");
            }
        }

        // Get a specific Competition by ID
        public async Task<IActionResult> HandleGetCompetition(string competitionId)
        {
            try
            {
                var competition = _context.CompetitionKoi
                    .Include(c => c.Award)
                    .Include(c => c.Referee).ThenInclude(cs => cs.User)
                    .Include(c => c.Category).ThenInclude(cs => cs.KoiStandard)
                    .Include(c => c.KoiRegistrations).ThenInclude(cs => cs.FishKoi)
                    .Include(c => c.Bets)
                    .FirstOrDefault(d => d.competition_id == competitionId);

                if (competition == null)
                {
                    return NotFound("Competition not found!");
                }

                return Ok(competition);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving competition: {ex.Message}");
            }
        }

        
    }
}
