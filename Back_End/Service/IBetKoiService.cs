using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using KoiBet.Data;
using KoiBet.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using KoiBet.DTO.Bet;
using KoiBet.DTO.BetKoi;
using KoiBet.DTO.User;

namespace Service.IBetKoiService
{
    // IBetKoiService Interface
   public interface IBetKoiService
    {
        // Đặt cược vào một con Koi
        Task<IActionResult> HandlePlaceBet(CreateBetDTO createBetDto);
        Task<IActionResult> HandleGetUserBets(string userId);
        Task<IActionResult> HandleUpdateBet(UpdateBetDTO updateBetDto);
        Task<IActionResult> HandleDeleteBet(string betId);
        Task<IActionResult> HandleGetBet(string betId);
    }

    // BetKoiService Implementation
    public class KoiBetService : ControllerBase, IBetKoiService
    {
        private readonly ApplicationDbContext _context;

        public KoiBetService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Đặt cược mới
        public async Task<IActionResult> HandlePlaceBet(CreateBetDTO createBetDto)
        {
            try
            {
                var lastBet = await _context.KoiBet
                    .OrderByDescending(b => b.bet_id)
                    .FirstOrDefaultAsync();

                int newBetNumber = 1;

                if (lastBet != null && lastBet.bet_id.StartsWith("bet_"))
                {
                    int.TryParse(lastBet.bet_id.Substring(4), out newBetNumber);
                    newBetNumber++;
                }

                string newBetId = $"Bet_{newBetNumber}";

                var bet = new BetKoi()
                {
                    bet_id = newBetId,
                    users_id = createBetDto.UserId,
                    registration_id = createBetDto.RegistrationId,
                    competition_id = createBetDto.CompetitionId,
                    bet_amount = createBetDto.BetAmount
                };

                _context.KoiBet.Add(bet);
                await _context.SaveChangesAsync();

                return Ok(bet);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                return StatusCode(500, $"Error occurred: {ex.Message}");
            }
        }

        // Lấy danh sách cược của người dùng
        public async Task<IActionResult> HandleGetUserBets(string userId)
        {
            var userBets = await _context.KoiBet
                .Include(b => b.Competition)
                .Include(b => b.User)
                .Where(b => b.users_id == userId)
                .ToListAsync();

            if (userBets == null || userBets.Count == 0)
            {
                return NotFound("No bets found for the specified user.");
            }

            return Ok(userBets);
        }

        // Lấy thông tin chi tiết của một cược
        public async Task<IActionResult> HandleGetBet(string betId)
        {
            var bet = await _context.KoiBet
                  .Include(b => b.Competition)
                  .Include(b => b.User)
                  .FirstOrDefaultAsync(b => b.bet_id == betId);

            if (bet == null)
            {
                return NotFound("Bet not found.");
            }

            // Kiểm tra các thuộc tính của bet có giá trị null hay không
            if (bet.Competition == null || bet.User == null)
            {
                return NotFound("Associated data not found.");
            }

            return Ok(bet);
        }

        // Cập nhật cược
        public async Task<IActionResult> HandleUpdateBet(UpdateBetDTO updateBetDto)
        {
            var bet = await _context.KoiBet.FindAsync(updateBetDto.BetId);
            if (bet == null)
            {
                return NotFound("Bet not found.");
            }

            bet.users_id = updateBetDto.UserId;
            bet.registration_id = updateBetDto.RegistrationId;
            bet.competition_id = updateBetDto.CompetitionId;
            bet.bet_amount = updateBetDto.BetAmount;

            _context.KoiBet.Update(bet);
            await _context.SaveChangesAsync();
            return Ok(bet);
        }

        // Xóa cược
        public async Task<IActionResult> HandleDeleteBet(string betId)
        {
            var bet = await _context.KoiBet.FindAsync(betId);
            if (bet == null)
            {
                return NotFound("Bet not found.");
            }

            _context.KoiBet.Remove(bet);
            await _context.SaveChangesAsync();
            return Ok("Bet successfully deleted.");
        }
    }
}
