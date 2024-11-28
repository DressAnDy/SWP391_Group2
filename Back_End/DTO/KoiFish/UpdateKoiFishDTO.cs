namespace DTO;

using KoiBet.DTO.User;

public class UpdateKoiFishDTO
{
    public string? koi_id { get; set; }

    public string koi_name { get; set; }

    public string koi_variety { get; set; }

    public string koi_size { get; set; }

    public string koi_age { get; set; }
}
