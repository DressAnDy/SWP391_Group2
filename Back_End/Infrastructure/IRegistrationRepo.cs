using KoiBet.Data;
using KoiBet.DTO;
using KoiBet.DTO.Competition;
using KoiBet.Entities;

namespace KoiBet.Infrastructure;

public interface IRegistrationRepo
{
    public int GenerateCompetitionMatch(KoiRegistration firstKoi, KoiRegistration secondKoi, string compeId);

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
            match_id = roundId + "_MATCH_" + matchCount + 1,
            round_id = roundId,
            result = "Pending",
            first_koiId1 = firstKoi.koi_id,
            first_koiId2 = secondKoi.koi_id,
        };

        _context.CompetitionMatch.Add(match);
        return _context.SaveChanges();
    }
}
