using KoiBet.DTO;
using DTO;
using KoiBet.DTO.Referee;
using KoiBet.DTO;
using System.ComponentModel.DataAnnotations;
using KoiBet.Entities;

namespace KoiBet.DTO.Competition
{
    public class CompetitionKoiDTO
    {
        [Key]
        public string CompetitionId { get; init; }

        public string CompetitionName { get; init; }

        public string? CompetitionDescription { get; init; }

        public DateTime? StartTime { get; init; }

        public DateTime? EndTime { get; init; }

        public string? StatusCompetition { get; init; }
        public string? Round { get; init; }
        public string? category_id { get; set; }
        public string? koi_id { get; set; }
        public string? referee_id { get; set; }
        public string? award_id { get; set; }
        public int number_attendees { get; set; }

        public KoiCategoryDTO? KoiCategory { get; set; }

        public KoiFishDTO? KoiFish { get; set; }

        public RefereeDTO? Referee { get; set; }

        public AwardDTO? Award { get; set; }
        public List<KoiRegistration>? KoiRegistrations { get; set; }

        public string? CompetitionImg { get; set; }
    }

    public class GetCompeByUserIdDTO
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string KoiId { get; set; }
        public string KoiName { get; set; }
        public string CompetitionId { get; set; }
        public string? Phone {  get; set; }
        public string? Email { get; set; }
    }

    public class CompetitionResponseDTO
    {
        public string CompetitionId { get; init; }

        public string CompetitionName { get; init; }

        public string? CompetitionDescription { get; init; }

        public DateTime? StartTime { get; init; }

        public DateTime? EndTime { get; init; }

        public string? StatusCompetition { get; init; }

        public string? Round { get; init; }
        //public string? CategoryId { get; set; }
        public string? KoiId { get; set; }
        //public string? RefereeId { get; set; }
        //public string? AwardId { get; set; }

        public KoiCategory KoiCategory { get; set; }

        public FishKoi? KoiFish { get; set; }

        //public Referee? Referee { get; set; }

        public Award Award { get; set; }

        public string? CompetitionImg { get; set; }
        public int number_attendees { get; set; }
    }
}
