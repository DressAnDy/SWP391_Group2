using KoiBet.Entities;

namespace KoiBet.DTO.User
{
    public class ManagerDTO
    {
        public Guid user_id { get; set; }

        public string? user_name { get; set; }

        public string? full_name { get; set; }

        public string? password { get; set; }

        public string? email { get; set; }

        public string? phone { get; set; }

        public string? role_id { get; set; }

        public double? balance { get; set; }
    }
}
