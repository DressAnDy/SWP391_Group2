using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KoiBet.Entities
{
    public class Transactions
    {
        [Key]
        public string transactions_id { get; set; }

        [Required]
        [ForeignKey("users_id")]
        public string? users_id { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime transactions_time { get; set; }

        [Required]
        public string? messages { get; set; }

        public virtual Users User { get; set; }

    }
}