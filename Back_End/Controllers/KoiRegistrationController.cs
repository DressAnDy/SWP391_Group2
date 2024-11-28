using KoiBet.DTO;
using KoiBet.Service;
using Microsoft.AspNetCore.Mvc;

namespace KoiBet.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class KoiRegistrationController : ControllerBase
    {
        private readonly IKoiRegistrationService _koiRegistrationService;

        public KoiRegistrationController(IKoiRegistrationService koiRegistrationService)
        {
            _koiRegistrationService = koiRegistrationService;
        }

        [HttpGet("Get All KoiRegistration")]
        public async Task<IActionResult> GetAllKoiRegistration()
        {
            return await _koiRegistrationService.HandleGetAllKoiRegistrations();
        }

        //[HttpGet("Get Registration Statistics")]
        //public async Task<IActionResult> GetRegistrationStatistics([FromQuery] string koiId)
        //{
        //    return await _koiRegistrationService.HandleGetRegistrationStatistics(koiId);
        //}

        [HttpGet("Get KoiRegistrationById")]
        public async Task<IActionResult> GetKoiRegistrationById(string koiRegistrationId)
        {
            return await _koiRegistrationService.HandleGetKoiRegistration(koiRegistrationId);
        }

        [HttpGet("Get KoiRegistrationByKoiId")]
        public async Task<IActionResult> GetKoiRegistrationByKoiId([FromQuery] string koiId)
        {
            return await _koiRegistrationService.HandleGetKoiRegistrationByKoiId(koiId);
        }

        [HttpGet("Get KoiRegistrationByCompetitionId")]
        public async Task<IActionResult> GetKoiRegistrationByCompetitionId([FromQuery] string competitionId)
        {
            return await _koiRegistrationService.HandleGetKoiRegistrationByCompetitionId(competitionId);
        }

        [HttpGet("Get KoiRegistrationByUserId")]
        public async Task<IActionResult> GetKoiRegistrationByUserId([FromQuery] string userId)
        {
            return await _koiRegistrationService.HandleGetKoiRegistrationByUserId(userId);
        }

        [HttpDelete("Delete KoiRegistration")]
        public async Task<IActionResult> DeleteAward(string koiRegistrationId)
        {
            return await _koiRegistrationService.HandleDeleteKoiRegistration(koiRegistrationId);
        }

        [HttpPost("Create KoiRegistration")]
        public async Task<IActionResult> CreateKoiRegistration([FromBody] CreateKoiRegistrationDTO createKoiRegistrationDto)
        {
            if (createKoiRegistrationDto == null)
            {
                return BadRequest("Invalid registration data.");
            }

            return await _koiRegistrationService.HandleCreateNewKoiRegistration(createKoiRegistrationDto);
        }

        [HttpPut("Update KoiRegistration")]
        public async Task<IActionResult> UpdateKoiRegistration([FromBody] UpdateKoiRegistrationDTO updateKoiRegistrationDto)
        {
            if (updateKoiRegistrationDto == null)
            {
                return BadRequest("Invalid update data.");
            }

            return await _koiRegistrationService.HandleUpdateKoiRegistration(updateKoiRegistrationDto);
        }


    }
}
