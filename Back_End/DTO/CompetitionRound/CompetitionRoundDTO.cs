using KoiBet.DTO.Competition;
using KoiBet.DTO;
using KoiBet.Entities;

namespace KoiBet.DTO.CompetitionRound
{
    public class CompetitionRoundDTO
    {
        public string RoundId { get; set; }
        public int Match { get; set; }
        public string CompetitionId { get; set; }

        public CompetitionKoi CompetitionKoi { get; set; }
        public ICollection<CompetitionMatch> Matches { get; set; } = new List<CompetitionMatch>();
    }

    public class CreateCompetitionRoundDTO
    {
        public int Match { get; set; }
        public string CompetitionId { get; set; }
    }

    public class UpdateCompetitionRoundDTO
    {
        public string RoundId { get; set; }
        public int Match { get; set; }
        public string CompetitionId { get; set; } 
    }
}

