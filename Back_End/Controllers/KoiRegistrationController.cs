using KoiBet.DTO.KoiRegistration;
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

        [HttpGet("Get KoiRegistrationById")]
        public async Task<IActionResult> GetKoiRegistrationById(string koiRegistrationId)
        {
            return await _koiRegistrationService.HandleGetKoiRegistration(koiRegistrationId);
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
        public async Task<IActionResult> UpdateKoiRegistration(string registrationId, [FromBody] UpdateKoiRegistrationDTO updateKoiRegistrationDto)
        {
            if (updateKoiRegistrationDto == null)
            {
                return BadRequest("Invalid update data.");
            }

            return await _koiRegistrationService.HandleUpdateKoiRegistration(registrationId, updateKoiRegistrationDto);
        }


    }
}
