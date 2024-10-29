using Microsoft.AspNetCore.Mvc;
using KoiBet.Data;
using KoiBet.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using KoiBet.DTO.Competition;
using KoiBet.Service;
using KoiBet.DTO;
using KoiBet.DTO.Referee;
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
                    .Select(competition => new CompetitionKoiDTO
                    {
                        CompetitionId = competition.competition_id,
                        CompetitionName = competition.competition_name,
                        CompetitionDescription = competition.competition_description,
                        StartTime = competition.start_time,
                        EndTime = competition.end_time,
                        StatusCompetition = competition.status_competition,
                        Round = competition.rounds,
                        category_id = competition.category_id,
                        koi_id = competition.koi_id,
                        referee_id = competition.referee_id,
                        award_id = competition.award_id,
                        CompetitionImg = competition.competition_img,
                        number_attendees = competition.number_attendees,
                    })
                    .ToListAsync();

                if (!competitions.Any())
                {
                    return NotFound("No competitions found!");
                }

                // Loop through each competition to get additional information for Koi, Referee, and Award
                foreach (var competition in competitions)
                {
                    // Get Koi Category based on category_id
                    if (!string.IsNullOrEmpty(competition.category_id?.ToString()))
                    {
                        var categoryResult = await _koiCategoryService.HandleGetKoiCategory(competition.category_id.ToString()) as OkObjectResult;
                        if (categoryResult?.Value is KoiCategoryDTO category)
                        {
                            competition.KoiCategory = category;
                        }
                    }

                    // Get KoiFish information based on koi_id
                    if (!string.IsNullOrEmpty(competition.koi_id?.ToString()))
                    {
                        var koiResult = await _koiService.HandleGetKoiFishById(new SearchKoiDTO { koi_id = competition.koi_id.ToString() }) as OkObjectResult;

                        if (koiResult?.Value is KoiFishDTO koi)
                        {
                            competition.KoiFish = koi;
                        }
                    }

                    // Get Referee information based on referee_id
                    if (!string.IsNullOrEmpty(competition.referee_id?.ToString()))
                    {
                        var refereeResult = await _refereeService.HandleGetReferee(competition.referee_id.ToString()) as OkObjectResult;
                        if (refereeResult?.Value is RefereeDTO referee)
                        {
                            competition.Referee = referee;
                        }
                    }

                    // Get Award information based on award_id
                    if (!string.IsNullOrEmpty(competition.award_id?.ToString()))
                    {
                        var awardResult = await _awardService.HandleGetAwardById(competition.award_id.ToString()) as OkObjectResult;
                        if (awardResult?.Value is AwardDTO award)
                        {
                            competition.Award = award;
                        }
                    }

                    //// Get KoiRegistration information based on koi_id
                    //var KoiRegistrations = _context.KoiRegistration
                    //    .Where(c => c.competition_id == competition.CompetitionId)
                    //    .ToList();

                    //competition.KoiRegistrations = KoiRegistrations;
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
                var lastCompetition = await _context.CompetitionKoi
                    .OrderByDescending(c => c.competition_id)
                    .FirstOrDefaultAsync();

                int newIdNumber = 1; // Default ID starts at 1 if no records exist

                if (lastCompetition != null)
                {
                    var lastId = lastCompetition.competition_id;
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
                    rounds = createCompetitionDto.rounds,
                    category_id = createCompetitionDto.category_id,
                    koi_id = createCompetitionDto.koi_id,
                    referee_id = createCompetitionDto.referee_id,
                    award_id = createCompetitionDto.award_id,
                    competition_img = createCompetitionDto.competition_img,
                    number_attendees = createCompetitionDto.number_attendees,
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
                var competition = await _context.CompetitionKoi
                    .FirstOrDefaultAsync(c => c.competition_id == updateCompetitionDto.CompetitionId);

                if (competition == null)
                {
                    return NotFound("Competition not found!");
                }

                competition.competition_name = updateCompetitionDto.CompetitionName;
                competition.competition_description = updateCompetitionDto.CompetitionDescription;
                DateTime startTime = competition.start_time;
                DateTime endTime = competition.end_time;
                competition.status_competition = updateCompetitionDto.StatusCompetition;
                competition.rounds = updateCompetitionDto.Round;
                competition.category_id = updateCompetitionDto.KoiCategoryId;
                competition.koi_id = updateCompetitionDto.KoiFishId;
                competition.referee_id = updateCompetitionDto.RefereeId;
                competition.award_id = updateCompetitionDto.AwardId;
                competition.competition_img = updateCompetitionDto.CompetitionImg;
                competition.number_attendees = updateCompetitionDto.number_attendees;

                _context.CompetitionKoi.Update(competition);

                if(competition.status_competition == "Active")
                {
                    for (int i = 0; i < competition.rounds; i++)
                    {
                        var round = new CompetitionRound
                        {
                            RoundId = updateCompetitionDto.CompetitionId + "_" + "RND_" + i,
                            Match = 2,
                            competition_id = updateCompetitionDto.CompetitionId
                        };

                        //var match_1 = new CompetitionMatch
                        //{
                        //    match_id = BCrypt.Net.BCrypt.HashPassword(id).Substring(0, 30),
                        //    first_koiId1 = "TEMP_1",
                        //    first_koiId2 = "TEMP_2",
                        //    round_id = round.RoundId,
                        //    result = "Pending"
                        //};

                        //var match_2 = new CompetitionMatch
                        //{
                        //    match_id = BCrypt.Net.BCrypt.HashPassword(id).Substring(0, 30),
                        //    first_koiId1 = "TEMP_1",
                        //    first_koiId2 = "TEMP_2",
                        //    round_id = round.RoundId,
                        //    result = "Pending"
                        //};

                        _context.CompetitionRound.Add(round);
                        //_context.CompetitionMatch.Add(match_1);
                        //_context.CompetitionMatch.Add(match_2);
                    }
                }
                var result = await _context.SaveChangesAsync();

                if (result == 0)
                {
                    return BadRequest("Failed to update competition!");
                }

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
                var competition = await _context.CompetitionKoi
                    .Select(c => new CompetitionKoiDTO
                    {
                        CompetitionId = c.competition_id,
                        CompetitionName = c.competition_name,
                        CompetitionDescription = c.competition_description,
                        StartTime = c.start_time,
                        EndTime = c.end_time,
                        StatusCompetition = c.status_competition,
                        Round = c.rounds,
                        category_id = c.category_id,
                        koi_id = c.koi_id,
                        referee_id = c.referee_id,
                        award_id = c.award_id,
                        CompetitionImg = c.competition_img,
                        number_attendees = c.number_attendees,
                    })
                    .FirstOrDefaultAsync(c => c.CompetitionId == competitionId);

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
