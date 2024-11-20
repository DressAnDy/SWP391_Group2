namespace KoiBet.DTO.KoiBetStatisticsDTO
{
    public class BetStatisticsDTO
    {
        public int TotalBets { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalWins { get; set; }
        public int TotalLosses { get; set; }
        public decimal TotalWinAmount { get; set; }
        public decimal TotalLoseAmount { get; set; }
    }
}
