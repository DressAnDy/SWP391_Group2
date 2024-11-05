using KoiBet.DTO.User;
using KoiBet.DTO.Competition;
using KoiBet.Entities;
using System.Text.Json.Serialization;

namespace KoiBet.DTO.BetKoi
{
    public class BetKoiDTO
    {
        public string BetId { get; set; }
        public string user_id { get; set; }
        public UserDTO User { get; set; }

        [JsonIgnore]
        public string competition_id { get; set; }
        public decimal bet_amount { get; set; }
        public CompetitionKoi CompetitionKoi { get; set; }
        public KoiRegistration KoiRegistration { get; set; }
    }
}
