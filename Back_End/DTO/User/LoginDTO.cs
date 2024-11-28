namespace KoiBet.DTO.User
{
    public class LoginDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }

    }

    public class PasswordChangeDTO
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}