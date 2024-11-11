using KoiBet.DTO.User;

namespace KoiBet.DTO
{
    public class TransactionsDTO
    {
        public string transactions_id { get; set; }

        public string users_id { get; set; }

        public decimal amount { get; set; }

        public string messages { get; set; }

        public DateTime transactions_time { get; set; }

        public UserDTO User { get; set; }
    }

    public class CreateTransactionsDTO
    {
        private string transactions_id { get; set; }

        public string users_id { get; set; }

        public decimal amount { get; set; }

        public string messages { get; set; }

        private DateTime transactions_time { get; set; }

        public UserDTO User { get; set; }
    }

    public class UpdateTransactionsDTO
    {
        private string transactions_id { get; set; }

        public string users_id { get; set; }

        public decimal amount { get; set; }

        public string messages { get; set; }

        public DateTime transactions_time { get; set; }

        public UserDTO User { get; set; }
    }
}
