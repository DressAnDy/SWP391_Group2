﻿using DTO;
using KoiBet.Data;
using KoiBet.DTO.Competition;
using KoiBet.DTO;
using KoiBet.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service.ICompetitionService;
using Service.KoiFishService;
using KoiBet.Infrastructure;

namespace KoiBet.Service
{
    public interface IKoiRegistrationService
    {
        Task<IActionResult> HandleGetAllKoiRegistrations();
        Task<IActionResult> HandleCreateNewKoiRegistration(CreateKoiRegistrationDTO createKoiRegistrationDto);
        Task<IActionResult> HandleUpdateKoiRegistration(UpdateKoiRegistrationDTO updateKoiRegistrationDto);
        Task<IActionResult> HandleDeleteKoiRegistration(string registrationId);
        Task<IActionResult> HandleGetKoiRegistration(string registrationId);
        Task<IActionResult> HandleGetKoiRegistrationByKoiId(string registrationId);
        Task<IActionResult> HandleGetKoiRegistrationByCompetitionId(string competitionId);
    }

    public class KoiRegistrationService : ControllerBase, IKoiRegistrationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<KoiRegistrationService> _logger;
        private readonly ICompetitionService _competitionService;
        private readonly IKoiCategoryService _koiCategoryService;
        private readonly IKoiFishService _koiService;
        private readonly IRegistrationRepo _registrationRepo;

        public KoiRegistrationService(ApplicationDbContext context, ILogger<KoiRegistrationService> logger, IKoiFishService koiService, ICompetitionService competitionService, IKoiCategoryService koiCategoryService)
        {
            _context = context;
            _logger = logger;
            _koiService = koiService;
            _koiCategoryService = koiCategoryService;
            _competitionService = competitionService;
            _registrationRepo = new RegistrationRepo(context);
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
                    StatusRegistration = "Pending",
                    CategoryId = createKoiRegistrationDto.CategoryId,
                    StartDates = DateTime.Now,
                    EndDates = DateTime.Now,
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
        public async Task<IActionResult> HandleUpdateKoiRegistration(UpdateKoiRegistrationDTO updateKoiRegistrationDto)
        {
            try
            {
                var registrationQuery = _context.KoiRegistration
                    .Where(c => c.competition_id ==  updateKoiRegistrationDto.CompetitionId)
                    .AsQueryable();

                // Validate input
                if (updateKoiRegistrationDto == null)
                {
                    _logger.LogWarning("UpdateKoiRegistrationDTO is null");
                    return BadRequest("Invalid input data");
                }

                // Kiểm tra registration tồn tại
                var registration = await _context.KoiRegistration
                    .Include(c => c.FishKoi)
                    .FirstOrDefaultAsync(r => r.RegistrationId == updateKoiRegistrationDto.RegistrationId);

                if (registration == null)
                {
                    _logger.LogWarning($"Registration not found with ID: {updateKoiRegistrationDto.RegistrationId}");
                    return NotFound("Koi registration not found!");
                }

                // Đếm số lượng attendees
                var attendeesCount = await registrationQuery
                    .Where(c => c.StatusRegistration == "Accepted")
                    .CountAsync();  // Sử dụng CountAsync thay vì ToList

                if (!string.IsNullOrEmpty(updateKoiRegistrationDto.CompetitionId))
                {
                    var competitionResult = await _competitionService.HandleGetCompetition(updateKoiRegistrationDto.CompetitionId) as OkObjectResult;
                    if (competitionResult?.Value is CompetitionKoiDTO competition)
                    {
                        if (attendeesCount >= competition.number_attendees)
                        {
                            return BadRequest("Competition is full!");
                        }
                    }
                }

                // Cập nhật thông tin registration
                registration.SlotRegistration = attendeesCount + 1;
                registration.koi_id = updateKoiRegistrationDto.KoiId;
                registration.competition_id = updateKoiRegistrationDto.CompetitionId;
                registration.StatusRegistration = updateKoiRegistrationDto.StatusRegistration;
                registration.CategoryId = updateKoiRegistrationDto.CategoryId;
                registration.StartDates = DateTime.Now;
                registration.EndDates = DateTime.Now;

                // Cập nhật vào database
                _context.KoiRegistration.Update(registration);

                if(registration.StatusRegistration == "Accepted")
                {
                    var user = _context.Users
                        .FirstOrDefault(c => c.user_id == registration.FishKoi.users_id);

                    if(registration.RegistrationFee > user.Balance)
                    {
                        return BadRequest("Not enough Money!");
                    }

                    user.Balance = user.Balance - registration.RegistrationFee;
                    _context.Users.Update(user);

                    if(registration.SlotRegistration %  2 == 0)
                    {
                        var koi_1 = registrationQuery
                            .FirstOrDefault(c => c.SlotRegistration == (registration.SlotRegistration - 1));

                        var response = _registrationRepo.GenerateCompetitionMatch(koi_1, registration, registration.competition_id);

                        if (response == 0) 
                        {
                            return NotFound("Unable to Update!");
                        }
                    }
                }
                
                var result = await _context.SaveChangesAsync();

                if (result != 0)
                {
                    _logger.LogError("Failed to update registration in database");
                    return BadRequest("Failed to update koi registration!");
                }

                return Ok("Update Successful!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HandleUpdateKoiRegistration");
                return StatusCode(500, $"Internal server error: {ex.Message}");
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

        // Get a specific KoiRegistration by KoiID
        public async Task<IActionResult> HandleGetKoiRegistrationByKoiId(string koiId)
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
                    .FirstOrDefaultAsync(r => r.KoiId == koiId);

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

        // Get a specific KoiRegistration by CompetitionID
        public async Task<IActionResult> HandleGetKoiRegistrationByCompetitionId(string competitionId)
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
                    .FirstOrDefaultAsync(r => r.CompetitionId == competitionId);

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
