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
            Task<IActionResult> HandleGetAllBet();
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
        //public async Task<IActionResult> HandlePlaceBet(CreateBetDTO createBetDto)
        //{
        //    try
        //    {
        //        // Lấy thông tin người dùng
        //        var user = await _context.Users
        //            .FirstOrDefaultAsync(u => u.user_id == createBetDto.UserId);

        //        if (user == null)
        //        {
        //            return BadRequest("User not found.");
        //        }

        //        // Kiểm tra số dư
        //        if (user.Balance < createBetDto.BetAmount)
        //        {
        //            return BadRequest("Insufficient balance.");
        //        }

        //        // Lấy thông tin đăng ký koi
        //        var registration = await _context.KoiRegistration
        //            .FirstOrDefaultAsync(r => r.koi_id == createBetDto.KoiId);

        //        if (registration == null)
        //        {
        //            return BadRequest("Registration not found.");
        //        }

        //        // Tiếp tục xử lý như cũ
        //        var match = await _context.CompetitionMatch
        //            .Include(m => m.Round)
        //                .ThenInclude(r => r.CompetitionKoi)
        //            .FirstOrDefaultAsync(m => m.match_id == createBetDto.MatchId);

        //        if (match == null || match.Round == null || match.Round.CompetitionKoi == null)
        //        {
        //            return BadRequest("Match or associated competition not found.");
        //        }

        //        var competitionId = match.Round.CompetitionKoi.competition_id;

        //        // Tạo mã Bet mới
        //        var lastBet = await _context.KoiBet
        //            .OrderByDescending(b => b.bet_id)
        //            .FirstOrDefaultAsync();

        //        int newBetNumber = 1;
        //        if (lastBet != null && lastBet.bet_id.StartsWith("Bet_"))
        //        {
        //            int.TryParse(lastBet.bet_id.Substring(4), out newBetNumber);
        //            newBetNumber++;
        //        }

        //        string newBetId = $"Bet_{newBetNumber}";

        //        // Kiểm tra đối tượng BetKoi đã tồn tại trong DbContext
        //        var existingBet = _context.KoiBet.Local.FirstOrDefault(b => b.bet_id == newBetId);
        //        if (existingBet != null)
        //        {
        //            // Tách đối tượng cũ ra khỏi DbContext
        //            _context.Entry(existingBet).State = EntityState.Detached;
        //        }

        //        // Tạo đối tượng Bet mới
        //        var bet = new BetKoi()
        //        {
        //            bet_id = newBetId,
        //            users_id = createBetDto.UserId,
        //            registration_id = registration.RegistrationId, // sử dụng registration_id từ KoiRegistration
        //            competition_id = competitionId,
        //            bet_amount = createBetDto.BetAmount
        //        };

        //        // Trừ số tiền đặt cược từ số dư của người dùng
        //        user.Balance -= createBetDto.BetAmount;

        //        // Lưu thay đổi vào cơ sở dữ liệu
        //        _context.KoiBet.Add(bet);
        //        await _context.SaveChangesAsync();

        //        return Ok(new
        //        {
        //            BetId = bet.bet_id,
        //            UserId = bet.users_id,
        //            MatchId = createBetDto.MatchId,
        //            KoiId = createBetDto.KoiId,
        //            RegistrationId = registration.RegistrationId,
        //            CompetitionId = competitionId,
        //            BetAmount = bet.bet_amount
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Xử lý lỗi nếu có
        //        return StatusCode(500, $"Error occurred: {ex.Message}");
        //    }
        //}


        public async Task<IActionResult> HandlePlaceBet(CreateBetDTO createBetDto)
        {
            try
            {
                // Lấy thông tin người dùng
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

                // Kiểm tra đối tượng BetKoi đã tồn tại trong DbContext
                var existingBet = _context.KoiBet.Local.FirstOrDefault(b => b.bet_id == newBetId);
                if (existingBet != null)
                {
                    // Tách đối tượng cũ ra khỏi DbContext
                    _context.Entry(existingBet).State = EntityState.Detached;
                }

                // Tạo đối tượng Bet mới
                var bet = new BetKoi()
                {
                    bet_id = newBetId,
                    users_id = createBetDto.UserId,
                    registration_id = createBetDto.RegistrationId,
                    competition_id = competitionId,
                    bet_amount = createBetDto.BetAmount
                };

                // Trừ số tiền đặt cược từ số dư của người dùng
                user.Balance -= createBetDto.BetAmount;

                // Lưu thay đổi vào cơ sở dữ liệu
                _context.KoiBet.Add(bet);
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

        // Lấy thông tin chi tiết của một cược

        //Điều chỉnh khi get lấy thêm koi_id
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

        }
    }