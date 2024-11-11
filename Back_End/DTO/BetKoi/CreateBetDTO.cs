namespace KoiBet.DTO.Bet
{
    public class CreateBetDTO
    {
        public string UserId { get; set; } = string.Empty;

        public string RegistrationId { get; set; } = string.Empty ;

        public string CompetitionId { get; set; } = string.Empty;

        public string MatchId {  get; set; } = string.Empty;

        public string KoiId {  get; set; } = string.Empty;
        public decimal BetAmount { get; set; }
    }
}
