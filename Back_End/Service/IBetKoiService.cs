using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using KoiBet.Data;
using KoiBet.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using KoiBet.DTO;
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
        Task<IActionResult> HandleGetAllBet();
        Task<IActionResult> HandleGetBetHistory(string userId);
        Task<IActionResult> UpdateBetStatus(string betId, string newStatus);

        Task<IActionResult> HandleGetBetStatistics();

    }

    public class KoiBetService : ControllerBase, IBetKoiService
    {
        private readonly ApplicationDbContext _context;

        public KoiBetService(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> UpdateBetStatus(string betId, string newStatus)
        {
            try
            {
                if (string.IsNullOrEmpty(betId) || string.IsNullOrEmpty(newStatus))
                {
                    return BadRequest("Invalid betId or status.");
                }

                var bet = await _context.KoiBet.FirstOrDefaultAsync(b => b.bet_id.Equals(betId));
                if (bet == null)
                {
                    return NotFound("Bet not found.");
                }

                bet.bet_status = newStatus;

                _context.KoiBet.Update(bet);
                await _context.SaveChangesAsync();

                return Ok("Bet status updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error occurred: {ex.Message}");
            }
        }

        public async Task<IActionResult> HandlePlaceBet(CreateBetDTO createBetDto)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.user_id == createBetDto.UserId);

                if (user == null)
                {
                    return BadRequest("User not found.");
                }

                // Kiểm tra số dư
                if (user.Balance < createBetDto.BetAmount)
                {
                    return BadRequest("Insufficient balance.");
                }

                // Lấy thông tin cuộc thi
                var match = await _context.CompetitionMatch
                    .Include(m => m.Round)
                        .ThenInclude(r => r.CompetitionKoi)
                    .FirstOrDefaultAsync(m => m.match_id == createBetDto.MatchId);

                if (match == null || match.Round == null || match.Round.CompetitionKoi == null)
                {
                    return BadRequest("Match or associated competition not found.");
                }
                else if (match.Round.CompetitionKoi.status_competition != "Active")
                {
                    return BadRequest("Competition is not available.");
                }

                var betValidate = await _context.KoiBet
                    .FirstOrDefaultAsync(c => c.users_id == createBetDto.UserId 
                    && c.registration_id == createBetDto.RegistrationId 
                    && c.competition_id == match.Round.CompetitionKoi.competition_id);

                if (betValidate != null)
                {
                    return BadRequest("You have already placed a bet.");
                }

                var competitionId = match.Round.CompetitionKoi.competition_id;

                // Tạo mã Bet mới
                var lastBet = await _context.KoiBet
                    .OrderByDescending(b => b.bet_id)
                    .FirstOrDefaultAsync();

                int newBetNumber = 1;
                if (lastBet != null && lastBet.bet_id.StartsWith("Bet_"))
                {
                    int.TryParse(lastBet.bet_id.Substring(4), out newBetNumber);
                    newBetNumber++;
                }

                string newBetId = $"Bet_{newBetNumber}";

                var existingBet = _context.KoiBet.Local.FirstOrDefault(b => b.bet_id == newBetId);
                if (existingBet != null)
                {
                    _context.Entry(existingBet).State = EntityState.Detached;
                }

                // Tạo đối tượng Bet mới
                var bet = new BetKoi()
                {
                    bet_id = newBetId,
                    users_id = createBetDto.UserId,
                    registration_id = createBetDto.RegistrationId,
                    competition_id = competitionId,
                    bet_amount = createBetDto.BetAmount,
                    bet_status = "Pending",
                    created_at = DateTime.Now,
                    payout_date = DateTime.Now,
                };

                // Trừ số tiền đặt cược từ số dư của người dùng
                user.Balance -= createBetDto.BetAmount;

                // Lưu thay đổi vào cơ sở dữ liệu
                _context.KoiBet.Add(bet);

                var newTranId = Guid.NewGuid().ToString();
                var hashedTranId = BCrypt.Net.BCrypt.HashPassword(newTranId).Substring(0, 50);

                var transaction = new Transactions
                {
                    transactions_id = hashedTranId,
                    users_id = createBetDto.UserId,
                    Amount = -createBetDto.BetAmount,
                    messages = "Place bet",
                    transactions_time = DateTime.Now
                };
                _context.Transactions.Add(transaction);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    BetId = bet.bet_id,
                    UserId = bet.users_id,
                    MatchId = createBetDto.MatchId,
                    KoiId = createBetDto.KoiId,
                    CompetitionId = competitionId,
                    BetAmount = bet.bet_amount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error occurred: {ex.Message}");
            }
        }

        // Lấy danh sách cược của người dùng
        public async Task<IActionResult> HandleGetUserBets(string userId)
        {
            var userBets = await _context.KoiBet
                .Include(b => b.Competition)
                .Include(b => b.User)
                .Include(b => b.KoiRegistration)
                .Where(b => b.users_id == userId)
                .Select(userBets => new
                {
                    BetId = userBets.bet_id,
                    User = userBets.User == null ? null : new UserDTO
                    {
                        user_id = userBets.User.user_id ?? string.Empty,
                        Username = userBets.User.Username ?? string.Empty,
                        full_name = userBets.User.full_name ?? string.Empty,
                        email = userBets.User.Email ?? string.Empty,
                        phone = userBets.User.Phone ?? string.Empty,
                        role_id = userBets.User.role_id ?? string.Empty,
                        balance = userBets.User.Balance ?? 0
                    },
                    CompetitionKoi = userBets.Competition == null ? null : new CompetitionKoi
                    {
                        competition_id = userBets.Competition.competition_id,
                        competition_name = userBets.Competition.competition_name ?? string.Empty,
                        competition_description = userBets.Competition.competition_description ?? string.Empty,
                        start_time = userBets.Competition.start_time,
                        end_time = userBets.Competition.end_time,
                        status_competition = userBets.Competition.status_competition ?? string.Empty,
                        category_id = userBets.Competition.category_id ?? string.Empty,
                        koi_id = userBets.Competition.koi_id ?? string.Empty,
                        referee_id = userBets.Competition.referee_id ?? string.Empty,
                        award_id = userBets.Competition.award_id ?? string.Empty,
                        rounds = userBets.Competition.rounds ?? string.Empty,
                        competition_img = userBets.Competition.competition_img ?? string.Empty,
                        number_attendees = userBets.Competition.number_attendees,
                    },
                    KoiRegistration = userBets.KoiRegistration == null ? null : new KoiRegistration
                    {
                        RegistrationId = userBets.KoiRegistration.RegistrationId ?? string.Empty,
                        StatusRegistration = userBets.KoiRegistration.StatusRegistration ?? string.Empty,
                        SlotRegistration = userBets.KoiRegistration.SlotRegistration,
                        RegistrationFee = userBets.KoiRegistration.RegistrationFee,
                        koi_id = userBets.KoiRegistration.koi_id ?? string.Empty,
                        competition_id = userBets.KoiRegistration.competition_id ?? string.Empty,
                        CategoryId = userBets.KoiRegistration.CategoryId ?? string.Empty
                    },
                })
                .ToListAsync();

            if (userBets == null || userBets.Count == 0)
            {
                return NotFound("No bets found for the specified user.");
            }

            return Ok(userBets);
        }

        public async Task<IActionResult> HandleGetBet(string betId)
        {
            try
            {
                var bet = await _context.KoiBet
                    .Include(b => b.User)
                    .Include(b => b.Competition)
                    .Include(b => b.KoiRegistration)
                    .FirstOrDefaultAsync(b => b.bet_id == betId);

                if (bet == null)
                {
                    return NotFound("Bet not found.");
                }

                var betDTO = new
                {
                    BetId = bet.bet_id,
                    User = new
                    {
                        user_id = bet.User?.user_id ?? string.Empty,
                        Username = bet.User?.Username ?? string.Empty,
                        full_name = bet.User?.full_name ?? string.Empty,
                        email = bet.User?.Email ?? string.Empty,
                        phone = bet.User?.Phone ?? string.Empty,
                        role_id = bet.User?.role_id ?? string.Empty,
                        balance = bet.User?.Balance ?? 0
                    },
                    Competition = new
                    {
                        competition_id = bet.Competition?.competition_id ?? string.Empty,
                        competition_name = bet.Competition?.competition_name ?? string.Empty,
                        competition_description = bet.Competition?.competition_description ?? string.Empty,
                        start_time = bet.Competition?.start_time ?? DateTime.MinValue,
                        end_time = bet.Competition?.end_time ?? DateTime.MinValue,
                        status_competition = bet.Competition?.status_competition ?? string.Empty,
                        number_attendees = bet.Competition?.number_attendees ?? 0
                    },
                    KoiRegistration = new
                    {
                        registrationId = bet.KoiRegistration?.RegistrationId ?? string.Empty,
                        koi_id = bet.KoiRegistration?.koi_id ?? string.Empty,
                        competition_id = bet.KoiRegistration?.competition_id ?? string.Empty,
                        statusRegistration = bet.KoiRegistration?.StatusRegistration ?? string.Empty,
                        slotRegistration = bet.KoiRegistration?.SlotRegistration ?? 0,
                        registrationFee = bet.KoiRegistration?.RegistrationFee ?? 0
                    }
                };

                return Ok(betDTO);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving bet: {ex.Message}");
            }
        }

        public async Task<IActionResult> HandleGetBetHistory(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("UserId is required.");
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.user_id == userId);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                var userBets = await _context.KoiBet
                    .Where(b => b.users_id == userId)
                    .Select(b => new
                    {
                        BetId = b.bet_id,
                        BetAmount = b.bet_amount,
                        Status = b.bet_status,
                        CompetitionId = b.competition_id,
                        RegistrationId = b.registration_id,
                        BetDate = b.created_at,
                        PayoutDate = b.payout_date,
                        InitialBalance = user.Balance
                    })
                    .OrderByDescending(b => b.BetDate)
                    .ToListAsync();

                if (userBets == null || userBets.Count == 0)
                {
                    return NotFound("No bet history found for the user.");
                }

                decimal? currentBalance = user.Balance;
                var betHistory = new List<object>();

                foreach (var bet in userBets)
                {
                    //// Cập nhật số dư theo trạng thái của cược
                    //if (bet.Status == "win")
                    //{
                    //    currentBalance += bet.BetAmount + (bet.BetAmount * 0.80m);
                    //}
                    //else if (bet.Status == "lose")
                    //{
                    //    currentBalance -= bet.BetAmount; // thua => trừ số tiền cược
                    //}

                    // Thêm thông tin cược vào lịch sử cược
                    betHistory.Add(new
                    {
                        BetId = bet.BetId,
                        BetAmount = bet.BetAmount,
                        Status = bet.Status,
                        CompetitionId = bet.CompetitionId,
                        RegistrationId = bet.RegistrationId,
                        BetDate = bet.BetDate,
                        PayoutDate = bet.PayoutDate,
                        CurrentBalance = currentBalance
                    });
                }

                user.Balance = currentBalance;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return Ok(betHistory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error occurred: {ex.Message}");
            }
        }

        // Cập nhật cược
        public async Task<IActionResult> HandleUpdateBet(UpdateBetDTO updateBetDto)
        {
            var betQuery = _context.KoiBet
                .AsQueryable();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.user_id == updateBetDto.UserId);

            var bet = betQuery
                .FirstOrDefault(c => c.bet_id == updateBetDto.BetId);

            if (bet == null)
            {
                return NotFound("Bet not found.");
            }

            if(bet.bet_status != "Pending")
            {
                if(bet.bet_status == "Disabled")
                {
                    return BadRequest("Bet is disabled!");
                }
                else
                {
                    return BadRequest("Competition is finished!");
                }
            }

            var betAmount = 0;

            if(updateBetDto.BetAmount > bet.bet_amount)
            {
                var amountDiff = updateBetDto.BetAmount - bet.bet_amount;

                if(user.Balance < amountDiff)
                {
                    return BadRequest("You don't have enough balance.");
                }
                user.Balance -= amountDiff;

                var newTranId = Guid.NewGuid().ToString();
                var hashedTranId = BCrypt.Net.BCrypt.HashPassword(newTranId).Substring(0, 50);

                var transaction = new Transactions
                {
                    transactions_id = hashedTranId,
                    users_id = user.user_id,
                    Amount = -amountDiff,
                    messages = "Update bet",
                    transactions_time = DateTime.Now
                };

                _context.Transactions.Add(transaction);
            }
            else if(updateBetDto.BetAmount < bet.bet_amount)
            {
                var amountDiff = bet.bet_amount - updateBetDto.BetAmount;
                user.Balance += amountDiff;

                var newTranId = Guid.NewGuid().ToString();
                var hashedTranId = BCrypt.Net.BCrypt.HashPassword(newTranId).Substring(0, 50);

                var transaction = new Transactions
                {
                    transactions_id = hashedTranId,
                    users_id = user.user_id,
                    Amount = +amountDiff,
                    messages = "Update bet",
                    transactions_time = DateTime.Now
                };

                _context.Transactions.Add(transaction);
            }

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

        public async Task<IActionResult> HandleGetAllBet()
        {
            try
            {
                var bets = await _context.KoiBet
                    .Include(b => b.User)
                    .Include(b => b.Competition)
                    .Include(b => b.KoiRegistration)
                    .Select(bet => new BetKoiDTO
                    {
                        BetId = bet.bet_id,
                        user_id = bet.User != null ? bet.User.user_id : null,
                        User = bet.User == null ? null : new UserDTO
                        {
                            user_id = bet.User.user_id ?? string.Empty,
                            Username = bet.User.Username ?? string.Empty,
                            full_name = bet.User.full_name ?? string.Empty,
                            email = bet.User.Email ?? string.Empty,
                            phone = bet.User.Phone ?? string.Empty,
                            role_id = bet.User.role_id ?? string.Empty,
                            balance = bet.User.Balance ?? 0
                        },
                        competition_id = bet.Competition != null ? bet.Competition.competition_id : null,
                        bet_amount = bet.bet_amount,
                        CompetitionKoi = bet.Competition == null ? null : new CompetitionKoi
                        {
                            competition_id = bet.Competition.competition_id,
                            competition_name = bet.Competition.competition_name ?? string.Empty,
                            competition_description = bet.Competition.competition_description ?? string.Empty,
                            start_time = bet.Competition.start_time,
                            end_time = bet.Competition.end_time,
                            status_competition = bet.Competition.status_competition ?? string.Empty,
                            category_id = bet.Competition.category_id ?? string.Empty,
                            koi_id = bet.Competition.koi_id ?? string.Empty,
                            referee_id = bet.Competition.referee_id ?? string.Empty,
                            award_id = bet.Competition.award_id ?? string.Empty,
                            rounds = bet.Competition.rounds ?? string.Empty,
                            competition_img = bet.Competition.competition_img ?? string.Empty,
                            number_attendees = bet.Competition.number_attendees,
                        },
                        KoiRegistration = bet.KoiRegistration == null ? null : new KoiRegistration
                        {
                            RegistrationId = bet.KoiRegistration.RegistrationId ?? string.Empty,
                            StatusRegistration = bet.KoiRegistration.StatusRegistration ?? string.Empty,
                            SlotRegistration = bet.KoiRegistration.SlotRegistration,
                            RegistrationFee = bet.KoiRegistration.RegistrationFee,
                            koi_id = bet.KoiRegistration.koi_id ?? string.Empty,
                            competition_id = bet.KoiRegistration.competition_id ?? string.Empty,
                            CategoryId = bet.KoiRegistration.CategoryId ?? string.Empty
                        }
                    })
                    .OrderByDescending(bet => bet.BetId)
                    .ToListAsync();

                if (bets.Count == 0)
                {
                    return NotFound("No bets found.");
                }

                return Ok(bets);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error occurred: {ex.Message}");
            }
        }

        public async Task<IActionResult> HandleGetBetStatistics()
        {
            try
            {
                var totalBets = await _context.KoiBet.CountAsync();

                var totalBetAmount = await _context.KoiBet.SumAsync(b => b.bet_amount);

                var betStatusCounts = await _context.KoiBet
                    .GroupBy(b => b.bet_status)
                    .Select(group => new
                    {
                        Status = group.Key,
                        Count = group.Count(),
                        TotalAmount = group.Sum(b => b.bet_amount)
                    })
                    .ToListAsync();

                var totalWinningAmount = await _context.KoiBet
                    .Where(b => b.bet_status == "Win")
                    .SumAsync(b => b.bet_amount);

                var totalLosingAmount = await _context.KoiBet
                    .Where(b => b.bet_status == "Lose")
                    .SumAsync(b => b.bet_amount);

                var statistics = new
                {
                    TotalBets = totalBets,
                    TotalBetAmount = totalBetAmount,
                    BetStatusCounts = betStatusCounts,
                    TotalWinningAmount = totalWinningAmount,
                    TotalLosingAmount = totalLosingAmount
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error occurred while fetching bet statistics: {ex.Message}");
            }
        }
    }
}