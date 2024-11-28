using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KoiBet.Entities
{
    public class KoiStandard
    {
        [Key]
        public string? standard_id { get; set; }

        public string? color_koi { get; set; }

        public string? pattern_koi { get; set; }

        public decimal? size_koi { get; set; }

        public int? age_koi { get; set; }

        public string? bodyshape_koi { get; set; }

        public string? variety_koi { get; set; }

        public string? standard_name { get; set; }

        public string? gender { get; set; }
        [JsonIgnore]
        public ICollection<KoiCategory> KoiCategories { get; set; }
    }
}