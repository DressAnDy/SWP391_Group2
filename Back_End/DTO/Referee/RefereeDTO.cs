using KoiBet.DTO.User;

namespace KoiBet.DTO
{
    public class RefereeDTO
    {
        public string RefereeId { get; set; }

        public string RefereeName { get; set; }

        public string ExpJudge { get; set; }

        public string UserId { get; set; }

    }

    public class CreateRefereeDTO
    {

        public string RefereeName { get; set; }

        public string ExpJudge { get; set; }

        public string? UsersId { get; set; }
    }

    public class UpdateRefereeDTO
    {
        public string RefereeId { get; set; }

        public string RefereeName { get; set; }

        public string ExpJudge { get; set; }

        public string? UsersId { get; set; }
    }
}
