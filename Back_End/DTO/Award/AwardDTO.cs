namespace KoiBet.DTO
{
    public class AwardDTO
    {
        public string award_id {  get; set; }

        public string award_name { get; set; }

        public int quantity { get; set; } = 0;

        //public string award_description { get; set; }
    }


    public class CreateAwardDTO {
        public string award_name { get; set; }

        public int quantity { get; set; }
    }

    public class UpdateAwardDTO
    {
        public string award_id {  get; set; }

        public string award_name { get;set; }

        public int quantity { get; set; }

    }
}

