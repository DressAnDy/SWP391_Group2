using KoiBet.Data;
using KoiBet.DTO;
using KoiBet.DTO.Competition;
using KoiBet.Entities;
using Microsoft.EntityFrameworkCore;
using System.Transactions;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace KoiBet.Infrastructure;

public interface IRegistrationRepo
{
    public int GenerateCompetitionMatch(KoiRegistration firstKoi, KoiRegistration secondKoi, string compeId);
    public int ProcessingBet(string koiRegistrationId);

}

public class RegistrationRepo : IRegistrationRepo
{
    private readonly ApplicationDbContext _context;

    public RegistrationRepo(ApplicationDbContext context)
    {
        _context = context;
    }

    public int GenerateCompetitionMatch(KoiRegistration firstKoi, KoiRegistration secondKoi, string compeId)
    {
        var roundId = compeId + "_RND_1";

        var matchCount = _context.CompetitionMatch
                    .Where(c => c.round_id == roundId)
                    .ToList()
                    .Count();

        var match = new CompetitionMatch
        {
            match_id = roundId + "_MATCH_" + (matchCount + 1),
            round_id = roundId,
            result = "Pending",
            first_koiId1 = firstKoi.koi_id,
            first_koiId2 = secondKoi.koi_id,
        };

        _context.CompetitionMatch.Add(match);
        return _context.SaveChanges();
    }

    public int ProcessingBet(string koiRegistrationId)
    {
        decimal betPot = 0;

        var regisQuery = _context.KoiRegistration
            .AsQueryable();

        var winnerKoi = _context.FishKoi
            .FirstOrDefault(c => c.koi_name == koiRegistrationId);

        var winnerRegis = _context.KoiRegistration
            .FirstOrDefault(c => c.koi_id == winnerKoi.koi_id);

        var regisList = regisQuery
            .Where(c => c.competition_id == winnerRegis.competition_id)
            .OrderBy(c => c.RegistrationId)
            .ToList();

        var betQuery = _context.KoiBet
            .AsQueryable();

        foreach (var regis in regisList)
        {
            var betList = betQuery
                .Include(c => c.User)
                .Where(c => c.registration_id == regis.RegistrationId)
                .ToList();

            foreach (var bet in betList)
            {
                if (bet.bet_status == "Pending")
                {
                    if (bet.registration_id == winnerRegis.RegistrationId)
                    {
                        bet.bet_status = "Win";
                        bet.payout_date = DateTime.Now;
                        _context.KoiBet.Update(bet);

                        var user = bet.User;
                        user.Balance += (bet.bet_amount * 2);
                        _context.Users.Update(user);

                        var newTranId = Guid.NewGuid().ToString();
                        var hashedTranId = BCrypt.Net.BCrypt.HashPassword(newTranId).Substring(0, 50);

                        var transaction = new Transactions
                        {
                            transactions_id = hashedTranId,
                            users_id = user.user_id,
                            Amount = +(bet.bet_amount * 0.80m),
                            messages = "Win bet",
                            transactions_time = DateTime.Now
                        };
                        _context.Transactions.Add(transaction);
                    }
                    else
                    {
                        bet.bet_status = "Lose";
                        bet.payout_date = DateTime.Now;
                        _context.KoiBet.Update(bet);

                        betPot += bet.bet_amount;

                        var newTranId = Guid.NewGuid().ToString();
                        var hashedTranId = BCrypt.Net.BCrypt.HashPassword(newTranId).Substring(0, 50);

                        var transactionUser = new Transactions
                        {
                            transactions_id = hashedTranId,
                            users_id = bet.users_id,
                            Amount = -bet.bet_amount,
                            messages = "Lose bet",
                            transactions_time = DateTime.Now
                        };
                        _context.Transactions.Add(transactionUser);
                    }
                }
            }

            if(regis.RegistrationId == winnerRegis.RegistrationId)
            {
                regis.StatusRegistration = "Win";
            }
            else
            {
                regis.StatusRegistration = "Lose";
            }
            _context.KoiRegistration.Update(regis);
        }

        var admin = _context.Users
            .FirstOrDefault(c => c.role_id == "R5");

        admin.Balance += betPot;
        _context.Users.Update(admin);

        var newTranId2 = Guid.NewGuid().ToString();
        var hashedTranId2 = BCrypt.Net.BCrypt.HashPassword(newTranId2).Substring(0, 50);

        var transactionAdmin = new Transactions
        {
            transactions_id = hashedTranId2,
            users_id = admin.user_id,
            Amount = -betPot,
            messages = "Process bet",
            transactions_time = DateTime.Now
        };
        _context.Transactions.Add(transactionAdmin);

        return _context.SaveChanges();
    }
}
