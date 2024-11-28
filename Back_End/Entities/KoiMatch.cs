using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KoiBet.Entities
{
    public class KoiMatch
    {
        [Key]
        [Column("match_id")]
        [MaxLength(50)]
        public string MatchId { get; set; } = string.Empty;

        [Column("first_koi")]
        [MaxLength(100)]
        public string FirstKoi { get; set; } = string.Empty;

        [Column("round_id")]
        [MaxLength(50)]
        public string RoundId { get; set; } = string.Empty;

        [Column("second_koi")]
        [MaxLength(100)]
        public string SecondKoi { get; set; } = string.Empty;

        [Column("result")]
        [MaxLength(100)]
        public string Result { get; set; } = string.Empty;

        //[ForeignKey("RoundId")]
        //public virtual KoiRound KoiRound { get; set; }
    }
}
