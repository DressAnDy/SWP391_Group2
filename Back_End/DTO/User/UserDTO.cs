using KoiBet.Entities;

namespace KoiBet.DTO.User
{
    public class UserDTO
    {
        public string user_id { get; set; }

        public string? Username { get; set; }

        public string? full_name { get; set; }

        public string? email { get; set; }

        public string? phone { get; set; }

        public string? role_id { get; set; }

        public decimal? balance { get; set; } = 0.0M;
    }

    public class UpdateUserRoleDTO
    {
        public string? userName { get; set; }
        public string? user_id { get; set; }
        public string? role_id { get; set; }
    }

    public class UserStatisticsDTO
    {
        public Dictionary<string, int> UserRoles { get; set; }
    }
}