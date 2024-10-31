using KoiBet.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace KoiBet.Entities;
public class Users
{
    [Key]
    [Column("users_id")]
    public string user_id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string full_name { get; set; } = string.Empty;

    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [Column("role_id")]
    public string role_id { get; set; } = string.Empty;

    [Column("balance")]
    public decimal? Balance { get; set; } = 0.0M;

    [JsonIgnore]
    public virtual Roles Role { get; set; }

    [JsonIgnore]
    public virtual List<BetKoi> BetKoi { get; set; } = new List<BetKoi>();

    [JsonIgnore]
    public virtual List<FishKoi> FishKoi { get; set; } = new List<FishKoi>();

    public ICollection<Transactions> Transactions { get; set; } = new List<Transactions>();
    [JsonIgnore]
    public ICollection<Referee> Referees { get; set; } = new List<Referee>();
}
