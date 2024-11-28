using DTO;
using KoiBet.DTO.Competition;
using KoiBet.DTO;
using KoiBet.Entities;

namespace KoiBet.DTO
{
    public class KoiRegistrationDTO
    {
        public string RegistrationId { get; set; }      // ID của bản đăng ký
        public string? KoiId { get; set; }               // ID của cá koi
        public string CompetitionId { get; set; }       // ID của cuộc thi
        public string StatusRegistration { get; set; }    // Trạng thái đăng ký
        public string CategoryId { get; set; }          // ID của hạng mục thi đấu
        public int SlotRegistration { get; set; }       // Số lượng slot đăng ký
        public DateTime? StartDates { get; set; }       // Ngày bắt đầu
        public DateTime? EndDates { get; set; }         // Ngày kết thúc
        public decimal RegistrationFee { get; set; }    // Phí đăng ký

        public CompetitionKoi competition {  get; set; }

        public KoiCategory koicategory { get; set; }

        public FishKoi koiFish { get; set; }
        public Users User {  get; set; }
    }
}
