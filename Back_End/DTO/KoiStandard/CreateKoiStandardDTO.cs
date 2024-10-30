namespace KoiBet.DTO
{
    public class CreateKoiStandardDTO
    {
        public string? color_koi { get; set; }
        public string? pattern_koi { get; set; }
        public decimal? size_koi { get; set; }
        public int? age_koi { get; set; } = 0;
        public string? bodyshape_koi { get; set; }
        public string? variety_koi { get; set; }
        public string? standard_name { get; set; }
        public string? gender { get; set; }
    }
}
