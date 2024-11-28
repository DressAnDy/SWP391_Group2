using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using KoiBet.DTO;
using KoiBet.Service;

namespace KoiBet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KoiCategoryController : ControllerBase
    {
        private readonly IKoiCategoryService _KoiCategoryService;

        public KoiCategoryController(IKoiCategoryService KoiCategoryService)
        {
            _KoiCategoryService = KoiCategoryService;
        }

        [HttpGet("Get all KoiCategory")]
        public async Task<IActionResult> GetAllKoiCategory()
        {
            return await _KoiCategoryService.HandleGetAllKoiCategories();
        }

        [HttpGet("Get KoiCategory")]
        public async Task<IActionResult> GetKoiCategory(string KoiCategoryId)
        {
            return await _KoiCategoryService.HandleGetKoiCategory(KoiCategoryId);
        }



        [HttpPost("Create KoiCategory")]
        public async Task<IActionResult> CreateKoiCategory([FromBody] CreateKoiCategoryDTO _createKoiCategory)
        {
            return await _KoiCategoryService.HandleCreateKoiCategory(_createKoiCategory);
        }

        [HttpPut("Update KoiCategory/{category_id}")]
        public async Task<IActionResult> UpdateKoiCategory([FromBody] UpdateKoiCategoryDTO _updateKoiCategory)
        {
            return await _KoiCategoryService.HandleUpdateKoiCategory(_updateKoiCategory);
        
        }

        [HttpDelete("Delete KoiCategory")]
        public async Task<IActionResult> DeleteKoiCategory(string KoiCategoryId)
        {
            return await _KoiCategoryService.HandleDeleteKoiCategory(KoiCategoryId);
        }
    }
}
