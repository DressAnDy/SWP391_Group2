using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KoiBet.Entities
{
    public class Award
    {
        [Key]
        public string award_id {get; set; }

        public string award_name {get; set; }

        public int quantity {get; set; }

        [JsonIgnore]
        public ICollection<CompetitionKoi> Competitions { get; set; }
    }
}
