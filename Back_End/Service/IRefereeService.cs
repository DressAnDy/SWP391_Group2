using KoiBet.Data;
using KoiBet.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KoiBet.Entities;
using System.Security.Cryptography.Xml;

namespace KoiBet.Service
{
    public interface IRefereeService
    {
        Task<IActionResult> HandleGetAllReferees();
        Task<IActionResult> HandleCreateNewReferee(CreateRefereeDTO createRefereeDto);
        Task<IActionResult> HandleUpdateReferee(string refereeId, UpdateRefereeDTO updateRefereeDto);
        Task<IActionResult> HandleDeleteReferee(string refereeId);
        Task<IActionResult> HandleGetReferee(string refereeId);
    }

    public class RefereeService : ControllerBase, IRefereeService
    {
        private readonly ApplicationDbContext _context;

        public RefereeService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all Referees
        public async Task<IActionResult> HandleGetAllReferees()
        {
            try
            {
                var referees = await _context.Referee
                    .Select(referee => new RefereeDTO
                    {
                        RefereeId = referee.RefereeId,
                        RefereeName = referee.RefereeName,
                        ExpJudge = referee.ExpJudge,
                        UserId = referee.user_id
                    })
                    .ToListAsync();

                if (!referees.Any())
                {
                    return NotFound("No referees found!");
                }

                return Ok(referees);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving referees: {ex.Message}");
            }
        }

        // Create a new Referee
        public async Task<IActionResult> HandleCreateNewReferee(CreateRefereeDTO createRefereeDTO)
        {
            try
            {
                var lastReferee = await _context.Referee
                    .OrderByDescending(r => r.RefereeId)
                    .FirstOrDefaultAsync();

                int newIdNumber = 1; // Mặc định là 1 khi không có bản dữ liệu

                if (lastReferee != null)
                {
                    var lastId = lastReferee.RefereeId;
                    if (lastId.StartsWith("REF_"))
                    {
                        int.TryParse(lastId.Substring(4), out newIdNumber); // Lấy phần tử phía sau thằng REF_X (x là số tự tăng)
                        newIdNumber++;
                    }
                }

                var newReferee = new Referee
                {
                    RefereeId = $"REF_{newIdNumber}", // Tạo ID mới với định dạng REF_X
                    RefereeName = createRefereeDTO.RefereeName,
                    ExpJudge = createRefereeDTO.ExpJudge,
                    user_id = createRefereeDTO.UsersId
                };

                _context.Referee.Add(newReferee);
                var result = await _context.SaveChangesAsync();

                if (result != 1)
                {
                    return BadRequest("Failed to create new referee!");
                }

                return Ok(newReferee);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // Update an existing Referee
        public async Task<IActionResult> HandleUpdateReferee(string refereeId, UpdateRefereeDTO updateRefereeDto)
        {
            try
            {
                var referee = await _context.Referee
                    .FirstOrDefaultAsync(r => r.RefereeId == refereeId);

                if (referee == null)
                {
                    return NotFound("Referee not found!");
                }

                referee.RefereeName = updateRefereeDto.RefereeName;
                referee.ExpJudge = updateRefereeDto.ExpJudge;
                referee.user_id = updateRefereeDto.UsersId;

                _context.Referee.Update(referee);
                var result = await _context.SaveChangesAsync();

                if (result != 1)
                {
                    return BadRequest("Failed to update referee!");
                }

                return Ok(referee);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating referee: {ex.Message}");
            }
        }

        // Delete a Referee
        public async Task<IActionResult> HandleDeleteReferee(string refereeId)
        {
            try
            {
                var referee = await _context.Referee
                    .FirstOrDefaultAsync(r => r.RefereeId == refereeId);

                if (referee == null)
                {
                    return NotFound("Referee not found!");
                }

                _context.Referee.Remove(referee);
                var result = await _context.SaveChangesAsync();

                if (result != 1)
                {
                    return BadRequest("Failed to delete referee!");
                }

                return Ok("Referee deleted successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting referee: {ex.Message}");
            }
        }

        // Get a specific Referee by ID
        public async Task<IActionResult> HandleGetReferee(string refereeId)
        {
            if (string.IsNullOrWhiteSpace(refereeId))
            {
                return BadRequest("Invalid referee ID!");
            }

            try
            {
                var referee = await _context.Referee
                    .Select(r => new RefereeDTO
                    {
                        RefereeId = r.RefereeId,
                        RefereeName = r.RefereeName,
                        ExpJudge = r.ExpJudge,
                        UserId = r.user_id
                    })
                    .FirstOrDefaultAsync(r => r.RefereeId == refereeId);

                if (referee == null)
                {
                    return NotFound("Referee not found!");
                }

                return Ok(referee);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving referee: {ex.Message}");
            }
        }
    }
}
