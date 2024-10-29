using KoiBet.DTO.Competition;
using KoiBet.DTO;

namespace KoiBet.DTO.CompetitionRound
{
    public class CompetitionRoundDTO
    {
        public string RoundId { get; set; }
        public int Match { get; set; }
        public string CompetitionId { get; set; }

        public CompetitionKoiDTO CompetitionKoi { get; set; }
        public ICollection<CompetitionMatchDTO> Matches { get; set; } = new List<CompetitionMatchDTO>();
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

