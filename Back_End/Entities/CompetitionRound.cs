using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KoiBet.Entities
{
    public class CompetitionRound
    {
        [Key]
        [Column("round_id")]
        [MaxLength(50)]
        public string RoundId { get; set; } = string.Empty;

        [Column("match")]
        public int Match { get; set; }

        [Column("competition_id")]
        [MaxLength(50)]
        public string competition_id { get; set; }

        // Navigation property
        [ForeignKey("competition_id")]
        public virtual CompetitionKoi CompetitionKoi { get; set;}
        public virtual ICollection<CompetitionMatch> Matches { get; set; } = new List<CompetitionMatch>();
    }
}
