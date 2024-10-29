using DTO;
using KoiBet.DTO.Competition;
using KoiBet.DTO;

namespace KoiBet.DTO
{
    public class KoiScoreDTO
    {
        public string ScoreId { get; set; }    
        public string KoiId { get; set; }           
        public string RefereeId { get; set; }      
        public string MatchId { get; set; } 
        public decimal Score { get; set; }
    }
}
