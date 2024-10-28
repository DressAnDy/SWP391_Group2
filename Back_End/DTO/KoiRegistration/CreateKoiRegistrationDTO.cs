namespace KoiBet.DTO
{
    public class CreateKoiRegistrationDTO
    {
        public string? KoiId { get; set; } 
        public string CompetitionId { get; set; }
        //public string StatusRegistration { get; set; }
        public string CategoryId { get; set; }
        //public int SlotRegistration { get; set; }
        //public DateTime? StartDates { get; set; }
        //public DateTime? EndDates { get; set; }
        public decimal RegistrationFee { get; set; }    
    }
}
