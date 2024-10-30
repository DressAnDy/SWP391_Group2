using KoiBet.Entities;

namespace KoiBet.DTO.Bet
{
    public class UpdateBetDTO
    {
        public string BetId { get; set; } = string.Empty;
        
        public string UserId { get; set; } = string.Empty;
        
        public string RegistrationId { get; set; } = string.Empty;
        
        public string CompetitionId { get; set; } = string.Empty;
        public decimal BetAmount { get; set; }
    }
}
