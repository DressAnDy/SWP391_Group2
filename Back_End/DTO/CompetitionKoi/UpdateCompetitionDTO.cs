namespace KoiBet.DTO.Competition
{
    public class UpdateCompetitionDTO
    {
        public string CompetitionId { get; set; }
        public string? CompetitionName { get; set; }

        public string? CompetitionDescription { get; set; }

        public string? StatusCompetition { get; set; }

        //public string? Round { get; set; }

        public string? KoiCategoryId { get; set; }

        public string? KoiFishId { get; set; }

        public string? RefereeId { get; set; }

        public string? AwardId { get; set; }

        public string? CompetitionImg { get; set; }
        public int number_attendees { get; init; }
    }
}
