using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KoiBet.Entities
{
    public class Referee
    {
        [Key]
        [Column("referee_id")]
        public string RefereeId { get; set; }

        [Column("referee_name")]
        public string RefereeName { get; set; }

        [Column("exp_judge")]
        public string ExpJudge { get; set; }

        [Column("users_id")]
        public string user_id { get; set; }

        // Thiết lập mối quan hệ với người dùng (Users)
        [ForeignKey("user_id")]
        public virtual Users User { get; set; }

        [JsonIgnore]
        public virtual ICollection<KoiScore> Scores { get; set; }

        [JsonIgnore]
        public virtual ICollection<CompetitionKoi> Competitions { get; set; }
    }
}
