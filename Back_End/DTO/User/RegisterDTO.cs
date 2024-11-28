using System.ComponentModel.DataAnnotations;

namespace KoiBet.DTO.User
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string confirmPassword { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        public string fullName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        public string email { get; set; }

        [Required(ErrorMessage = "Phone is required.")]
        public string phone { get; set; }

        public decimal? Balance { get; set; } = 0.0M;
    }
}
