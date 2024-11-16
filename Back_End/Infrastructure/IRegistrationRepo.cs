using KoiBet.Data;
using KoiBet.DTO;
using KoiBet.DTO.Competition;
using KoiBet.Entities;

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

        var regisList = regisQuery
            .Where(c => c.competition_id == koiRegistrationId)
            .OrderBy(c => c.RegistrationId)
            .ToList();

        var betQuery = _context.KoiBet
            .AsQueryable();

        foreach(var regis in regisList)
        {
            var betList = betQuery
                .Where(c => c.registration_id == regis.RegistrationId)
                .ToList();

            foreach(var bet in betList)
            {
                if(bet.bet_status == "Pending")
                {
                    if(bet.registration_id == koiRegistrationId)
                    {
                        bet.bet_status = "Win";
                        bet.payout_date = DateTime.Now;
                        _context.KoiBet.Update(bet);

                        var user = bet.User;
                        user.Balance += (bet.bet_amount * 2);
                        _context.Users.Update(user);
                    }
                    else
                    {
                        bet.bet_status = "Lose";
                        bet.payout_date = DateTime.Now;
                        _context.KoiBet.Update(bet);

                        var manager = _context.Users
                            .FirstOrDefault(c => c.role_id == "R4");

                        manager.Balance += bet.bet_amount;
                        _context.Users.Update(manager);
                    }
                }
            }
        }

        return _context.SaveChanges();
    }
}
