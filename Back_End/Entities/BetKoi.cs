using System.ComponentModel.DataAnnotations;
using KoiBet.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace KoiBet.Entities
{
    public class BetKoi
    {
        [Key]
        public string bet_id { get; set; } = string.Empty;

        [Column("users_id")]
        public string users_id { get; set; } = string.Empty;

        [Column("registration_id")]
        public string registration_id { get; set; } = string.Empty;

        [Column("competition_id")]
        public string competition_id { get; set; } = string.Empty;

        public decimal bet_amount { get; set; }

        [JsonIgnore]
        public virtual Users User { get; set; }
        public virtual KoiRegistration KoiRegistration { get; set; }
        public virtual CompetitionKoi Competition { get; set; }
    }
}
