using DTO.KoiFish;
using KoiBet.DTO.KoiStandard;
using KoiBet.Entities;

namespace KoiBet.DTO
{
    public class KoiCategoryDTO
    {
        public string? category_id {  get; set; }

        public string? category_name { get; set; }

        public string? standard_id { get; set; }

        public KoiStandardDTO Standard { get; set; }
    }
}
