using KoiBet.DTO.CompetitionRound;

namespace KoiBet.DTO
{
    public class CompetitionMatchDTO
    {
        public string MatchId { get; set; }
        public string FishkoiId_1 { get; set; }
        public string RoundId { get; set; }
        public string FishkoiId_2 { get; set; }
        public string Result { get; set; }

        public CompetitionRoundDTO Round { get; set; }
    }

    public class CreateCompetitionMatchDTO
    {
        public string FishkoiId_1 { get; set; }
        public string RoundId { get; set; }
        public string FishkoiId_2 { get; set; }
        public string Result { get; set; }
    }

    public class UpdateCompetitionMatchDTO
    {
        public string MatchId { get; set; }
        public string FishkoiId_1 { get; set; }
        public string RoundId { get; set; }
        public string FishkoiId_2 { get; set; }
        public string Result { get; set; }
    }

    public class ProcessingMatchDTO
    {
        public string RoundId { get; set; }
    }
}
