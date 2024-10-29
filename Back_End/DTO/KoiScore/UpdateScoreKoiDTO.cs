using DTO;
using KoiBet.DTO.Competition;
using KoiBet.DTO;

namespace KoiBet.DTO
{
    public class UpdateKoiScoreDTO
    {
        public string ScoreId { get; set; }
        public string KoiId { get; set; }
        public string MatchId { get; set; }
        public decimal Score { get; set; }
    }
}
