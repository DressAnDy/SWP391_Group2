using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KoiBet.Entities
{
    public class KoiScore
    {
        [Key]
        [Column("score_id")]
        [MaxLength(50)]
        public string score_id { get; set; } = string.Empty;

        [Column("koi_id")]
        [MaxLength(50)]
        public string koi_id { get; set; } = string.Empty;

        [Column("referee_id")]
        [MaxLength(50)]
        public string referee_id { get; set; } = string.Empty;

        [Column("match_id")]
        [MaxLength(50)]
        public string match_id { get; set; } = string.Empty;

        [Column("score_koi")]
        [DataType(DataType.Currency)]
        public decimal score_koi { get; set; }

        // Navigation properties
        public virtual FishKoi FishKoi { get; set; }
        public virtual Referee Referee { get; set; }
        [JsonIgnore]
        public virtual CompetitionMatch CompetitionMatch { get; set; }

    }
}
