using KoiBet.Entities;

namespace KoiBet.DTO.Competition
{
    public class CreateCompetitionDTO
    {
        public string competition_name { get; set; }
        public string competition_description { get; set; }
        public DateTime start_time { get; set; }
        public DateTime end_time { get; set; }
        public string status_competition { get; set; }
        public string category_id { get; set; }
        public string koi_id { get; set; }
        public string referee_id { get; set; }
        public string award_id { get; set; }
        public int rounds { get; set; }
        public string competition_img { get; set; }
        public int number_attendees { get; set; }
    }
}
