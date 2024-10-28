using KoiBet.DTO.User;
using KoiBet.DTO.Competition;
using KoiBet.Entities;

namespace KoiBet.DTO.BetKoi
{
    public class BetKoiDTO
    {
        public string BetId { get; set; }

        
        public string user_id { get; set; }

        public UserDTO? User { get; set; } 

        public string registration_id { get; set; }

        public string RegistrationId { get; set; }

        public string competition_id { get; set; }
        public decimal bet_amount { get; set; }


        public CompetitionKoi? CompetitionKoi { get; set; } 
    }
}
