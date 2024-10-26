using DTO.KoiFish;
using KoiBet.Data;
using KoiBet.DTO.Competition;
using KoiBet.DTO.KoiCategory;
using KoiBet.DTO.KoiRegistration;
using KoiBet.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service.ICompetitionService;
using Service.KoiFishService;

namespace KoiBet.Service
{
    public interface IKoiRegistrationService
    {
        Task<IActionResult> HandleGetAllKoiRegistrations();
        Task<IActionResult> HandleCreateNewKoiRegistration(CreateKoiRegistrationDTO createKoiRegistrationDto);
        Task<IActionResult> HandleUpdateKoiRegistration(string registrationId, UpdateKoiRegistrationDTO updateKoiRegistrationDto);
        Task<IActionResult> HandleDeleteKoiRegistration(string registrationId);
        Task<IActionResult> HandleGetKoiRegistration(string registrationId);
    }

    public class KoiRegistrationService : ControllerBase, IKoiRegistrationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<KoiRegistrationService> _logger;
        private readonly ICompetitionService _competitionService;
        private readonly IKoiCategoryService _koiCategoryService;
        private readonly IKoiFishService _koiService;


        public KoiRegistrationService(ApplicationDbContext context, ILogger<KoiRegistrationService> logger, IKoiFishService koiService, ICompetitionService competitionService, IKoiCategoryService koiCategoryService)
        {
            _context = context;
            _logger = logger;
            _koiService = koiService;
            _koiCategoryService = koiCategoryService;
            _competitionService = competitionService;
        }

        // Get all KoiRegistrations
        public async Task<IActionResult> HandleGetAllKoiRegistrations()
        {
            try
            {
                var registrations = await _context.KoiRegistration
                    .Select(reg => new KoiRegistrationDTO
                    {
                        RegistrationId = reg.RegistrationId,
                        KoiId = reg.koi_id,
                        CompetitionId = reg.competition_id,
                        StatusRegistration = reg.StatusRegistration,
                        CategoryId = reg.CategoryId,
                        SlotRegistration = reg.SlotRegistration,
                        StartDates = reg.StartDates,
                        EndDates = reg.EndDates,
                        RegistrationFee = reg.RegistrationFee
                    })
                    .ToListAsync();

                foreach(var registration in registrations)
                {
                    if (!string.IsNullOrEmpty(registration.CompetitionId))
                    {
                        var competitionResult = await _competitionService.HandleGetCompetition(registration.CompetitionId) as OkObjectResult;
                        if(competitionResult?.Value is CompetitionKoiDTO competition)
                        {
                            registration.competition = competition;
                        }
                    }

                    if (!string.IsNullOrEmpty(registration.CategoryId))
                    {
                        var koicategoryResult = await _koiCategoryService.HandleGetKoiCategory(registration.CategoryId) as OkObjectResult;
                        if(koicategoryResult?.Value is KoiCategoryDTO koiCategory)
                        {
                            registration.koicategory = koiCategory;
                        }
                    }

                    if (!string.IsNullOrEmpty(registration.KoiId))
                    {
                        var fishkoiResult = await _koiService.HandleGetKoiFishById(new SearchKoiDTO { koi_id = registration.KoiId.ToString() }) as OkObjectResult;
                        if(fishkoiResult?.Value is KoiFishDTO koiFish)
                        {
                            registration.koiFish = koiFish;
                        }
                    }
                }


                if (!registrations.Any())
                {
                    return NotFound("No koi registrations found!");
                }

                return Ok(registrations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving koi registrations");
                return BadRequest($"Error retrieving koi registrations: {ex.Message}");
            }
        }

        // Create a new KoiRegistration
        public async Task<IActionResult> HandleCreateNewKoiRegistration(CreateKoiRegistrationDTO createKoiRegistrationDto)
        {
            try
            {
                var lastRegistration = await _context.KoiRegistration
                    .OrderByDescending(r => r.RegistrationId)
                    .FirstOrDefaultAsync();

                int newIdNumber = 1; // Mặc định ID bắt đầu từ 1 nếu không có bản ghi nào

                if (lastRegistration != null)
                {
                    var lastId = lastRegistration.RegistrationId;
                    if (lastId.StartsWith("REG_"))
                    {
                        int.TryParse(lastId.Substring(4), out newIdNumber); // Tăng số ID sau 'REG_'
                        newIdNumber++;
                    }
                }

                var newKoiRegistration = new KoiRegistration
                {
                    RegistrationId = $"REG_{newIdNumber}",
                    koi_id = createKoiRegistrationDto.KoiId,
                    competition_id = createKoiRegistrationDto.CompetitionId,
                    StatusRegistration = createKoiRegistrationDto.StatusRegistration,
                    CategoryId = createKoiRegistrationDto.CategoryId,
                    SlotRegistration = createKoiRegistrationDto.SlotRegistration,
                    StartDates = createKoiRegistrationDto.StartDates,
                    EndDates = createKoiRegistrationDto.EndDates,
                    RegistrationFee = createKoiRegistrationDto.RegistrationFee
                };

                _context.KoiRegistration.Add(newKoiRegistration);
                var result = await _context.SaveChangesAsync();

                if (result != 1)
                {
                    return BadRequest("Failed to create new koi registration!");
                }

                return Ok(newKoiRegistration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating koi registration");
                return BadRequest($"Error creating koi registration: {ex.Message}");
            }
        }

        // Update an existing KoiRegistration
        public async Task<IActionResult> HandleUpdateKoiRegistration(string registrationId, UpdateKoiRegistrationDTO updateKoiRegistrationDto)
        {
            try
            {
                var registration = await _context.KoiRegistration
                    .FirstOrDefaultAsync(r => r.RegistrationId == registrationId);

                if (registration == null)
                {
                    return NotFound("Koi registration not found!");
                }

                registration.koi_id = updateKoiRegistrationDto.KoiId;
                registration.competition_id = updateKoiRegistrationDto.CompetitionId;
                registration.StatusRegistration = updateKoiRegistrationDto.StatusRegistration;
                registration.CategoryId = updateKoiRegistrationDto.CategoryId;
                registration.SlotRegistration = updateKoiRegistrationDto.SlotRegistration;
                registration.StartDates = updateKoiRegistrationDto.StartDates;
                registration.EndDates = updateKoiRegistrationDto.EndDates;
                registration.RegistrationFee = updateKoiRegistrationDto.RegistrationFee;

                _context.KoiRegistration.Update(registration);
                var result = await _context.SaveChangesAsync();

                if (result != 1)
                {
                    return BadRequest("Failed to update koi registration!");
                }

                return Ok(registration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating koi registration");
                return BadRequest($"Error updating koi registration: {ex.Message}");
            }
        }

        // Delete a KoiRegistration
        public async Task<IActionResult> HandleDeleteKoiRegistration(string registrationId)
        {
            try
            {
                var registration = await _context.KoiRegistration
                    .FirstOrDefaultAsync(r => r.RegistrationId == registrationId);

                if (registration == null)
                {
                    return NotFound("Koi registration not found!");
                }

                _context.KoiRegistration.Remove(registration);
                var result = await _context.SaveChangesAsync();

                if (result != 1)
                {
                    return BadRequest("Failed to delete koi registration!");
                }

                return Ok("Koi registration deleted successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting koi registration");
                return BadRequest($"Error deleting koi registration: {ex.Message}");
            }
        }

        // Get a specific KoiRegistration by ID
        public async Task<IActionResult> HandleGetKoiRegistration(string registrationId)
        {
            try
            {
                var registration = await _context.KoiRegistration
                    .Select(r => new KoiRegistrationDTO
                    {
                        RegistrationId = r.RegistrationId,
                        KoiId = r.koi_id,
                        CompetitionId = r.competition_id,
                        StatusRegistration = r.StatusRegistration,
                        CategoryId = r.CategoryId,
                        SlotRegistration = r.SlotRegistration,
                        StartDates = r.StartDates,
                        EndDates = r.EndDates,
                        RegistrationFee = r.RegistrationFee
                    })
                    .FirstOrDefaultAsync(r => r.RegistrationId == registrationId);

                if (registration == null)
                {
                    return NotFound("Koi registration not found!");
                }

                return Ok(registration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving koi registration");
                return BadRequest($"Error retrieving koi registration: {ex.Message}");
            }
        }
    }
}
