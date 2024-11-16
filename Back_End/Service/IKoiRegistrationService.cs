using DTO;
using KoiBet.Data;
using KoiBet.DTO.Competition;
using KoiBet.DTO;
using KoiBet.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service.ICompetitionService;
using Service.KoiFishService;
using KoiBet.Infrastructure;
using System.Runtime.InteropServices;

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
                        RegistrationFee = reg.RegistrationFee,
                        koiFish = reg.FishKoi,
                        User = reg.FishKoi.User,
                        competition = reg.CompetitionKoi,
                        koicategory = reg.KoiCategory,
                    })
                    .ToListAsync();

                


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
                var regisQuery = _context.KoiRegistration
                    .AsQueryable();

                var compeQuery = _context.CompetitionKoi
                    .AsQueryable();

                var regisValidate = regisQuery
                    .Where(c => c.koi_id == createKoiRegistrationDto.KoiId)
                    .ToList();

                foreach (var regis in regisValidate)
                {
                    var compe = compeQuery
                        .FirstOrDefault(c => c.competition_id == regis.competition_id);

                    if(compe != null)
                    {
                        if (compe.status_competition == "Active" && regis.StatusRegistration == "Accepted")
                        {
                            return BadRequest("Already in a competition!");
                        }
                        else if(compe.competition_id == createKoiRegistrationDto.CompetitionId)
                        {
                            return BadRequest("Already in this competition!");
                        }
                    }
                }

                var compeValidate = compeQuery
                    .FirstOrDefault(c => c.competition_id == createKoiRegistrationDto.CompetitionId);

                if(compeValidate == null)
                {
                    return BadRequest("Competition not found!");
                }
                else if(compeValidate.status_competition != "Active")
                {
                    return BadRequest("Competition is not available!");
                }

                var lastRegistration = regisQuery
                    .OrderByDescending(r => r.RegistrationId)
                    .FirstOrDefault();

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

                // Kiểm tra registration tồn tại
                var registration = registrationQuery
                    .Include(c => c.FishKoi)
                    .FirstOrDefault(r => r.RegistrationId == updateKoiRegistrationDto.RegistrationId);

                if (registration == null)
                {
                    return NotFound("Koi registration not found!");
                }

                // Đếm số lượng attendees
                var attendeesCount = registrationQuery
                    .Where(c => c.StatusRegistration == "Accepted")
                    .ToList()
                    .Count();  // Sử dụng CountAsync thay vì ToList

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
                
                await _context.SaveChangesAsync();

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
                        RegistrationFee = r.RegistrationFee,
                        koiFish = r.FishKoi,
                        User = r.FishKoi.User,
                        competition = r.CompetitionKoi,
                        koicategory = r.KoiCategory,
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
                        RegistrationFee = r.RegistrationFee,
                        koiFish = r.FishKoi,
                        User = r.FishKoi.User,
                        competition = r.CompetitionKoi,
                        koicategory = r.KoiCategory,
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
                        RegistrationFee = r.RegistrationFee,
                        koiFish = r.FishKoi,
                        User = r.FishKoi.User,
                        competition = r.CompetitionKoi,
                        koicategory = r.KoiCategory,
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
