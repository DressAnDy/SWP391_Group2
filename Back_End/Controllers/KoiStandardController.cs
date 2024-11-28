using Microsoft.AspNetCore.Mvc;
using KoiBet.DTO;
using Service.KoiStandardService;

namespace KoiBet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KoiStandardController : ControllerBase
    {
        private readonly IKoiStandardService _koiStandardService;

        public KoiStandardController(IKoiStandardService koiStandardService)
        {
            _koiStandardService = koiStandardService;
        }

        [HttpGet("Get All KoiStandard")]
        public async Task<IActionResult> GetAllKoiStandards()
        {
            return await _koiStandardService.HandleGetAllKoiStandards();
        }

        [HttpGet("Get KoiStandard")]
        public async Task<IActionResult> GetKoiStandard(string standardId)
        {
            return await _koiStandardService.HandleGetKoiStandard(standardId);
        }


        [HttpPut("Update KoiStandard/{standard_id}")]
        public async Task<IActionResult> UpdateKoiStandard([FromBody] UpdateKoiStandardDTO updateKoiStandardDto)
        {
            return await _koiStandardService.HandleUpdateKoiStandard(updateKoiStandardDto);
        }

        [HttpDelete("Delete KoiStandard")]
        public async Task<IActionResult> DeleteKoiStandard(string standardId)
        {
            return await _koiStandardService.HandleDeleteKoiStandard(standardId);
        }

        [HttpPost("CreateKoiStandard")]
        public async Task<IActionResult> CreateKoiStandard([FromBody] CreateKoiStandardDTO _createKoiStandardDTO)
        {
            return await _koiStandardService.HandleCreateNewKoiStandard(_createKoiStandardDTO);
        }
    }
}
