namespace DTO;

using KoiBet.DTO.User;

public class KoiFishDTO
{
    public string? koi_id {  get; set; }

    public string koi_name { get; set; }

    public string koi_variety { get; set; }

    public string koi_size { get; set; }

    public string koi_age { get; set; }

    public string? userId { get; set; }

    public UserDTO? users_id { get; set; }
}

public class SearchKoiDTO
{
    public string koi_id { get; set; }
}

public class SearchKoiUserIdDTO
{
    public string user_id { get; set; }
}
