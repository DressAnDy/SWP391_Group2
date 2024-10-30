using Microsoft.AspNetCore.Mvc;
using KoiBet.Data;
using KoiBet.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using KoiBet.DTO;

namespace Service.KoiStandardService
{
    public interface IKoiStandardService
    {
        Task<IActionResult> HandleGetAllKoiStandards();
        Task<IActionResult> HandleCreateNewKoiStandard(CreateKoiStandardDTO createKoiStandardDto);
        Task<IActionResult> HandleUpdateKoiStandard(UpdateKoiStandardDTO updateKoiStandardDto);
        Task<IActionResult> HandleDeleteKoiStandard(string standardId);
        Task<IActionResult> HandleGetKoiStandard(string standardId);
    }

    public class KoiStandardService : ControllerBase, IKoiStandardService
    {
        private readonly ApplicationDbContext _context;

        public KoiStandardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> HandleGetAllKoiStandards()
        {
            try
            {
                var koiStandards = await _context.KoiStandard
                    .Select(standard => new KoiStandardDTO
                    {
                        standard_id = standard.standard_id,
                        color_koi = standard.color_koi,
                        pattern_koi = standard.pattern_koi,
                        size_koi = standard.size_koi,
                        age_koi = standard.age_koi,
                        bodyshape_koi = standard.bodyshape_koi,
                        variety_koi = standard.variety_koi,
                        standard_name = standard.standard_name,
                        gender = standard.gender
                    })
                    .ToListAsync();

                if (koiStandards.Count < 1)
                {
                    return BadRequest("No standards found!");
                }

                return Ok(koiStandards);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public async Task<IActionResult> HandleCreateNewKoiStandard(CreateKoiStandardDTO createKoiStandardDto)
        {
            try
            {
                var lastKoiStandard = await _context.KoiStandard
                    .OrderByDescending(k => k.standard_id)
                    .FirstOrDefaultAsync();

                int newIdNumber = 1; //Mặc định là 1 khi ko có bản dữ liệu

                if (lastKoiStandard != null)
                {
                    var lastId = lastKoiStandard.standard_id;
                    if (lastId.StartsWith("KST_"))
                    {
                        int.TryParse(lastId.Substring(4), out newIdNumber); //lấy phần tử phía sau thằng KST_X ( x là number tự tăng )
                        newIdNumber++;
                    }
                }


                var newKoiStandard = new KoiStandard
                {
                    standard_id = $"KST_{newIdNumber}",
                    color_koi = createKoiStandardDto.color_koi,
                    pattern_koi = createKoiStandardDto.pattern_koi,
                    size_koi = createKoiStandardDto.size_koi,
                    age_koi = createKoiStandardDto.age_koi,
                    bodyshape_koi = createKoiStandardDto.bodyshape_koi,
                    variety_koi = createKoiStandardDto.variety_koi,
                    standard_name = createKoiStandardDto.standard_name,
                    gender = createKoiStandardDto.gender,
                    KoiCategories = new List<KoiCategory>()
                };

                _context.KoiStandard.Add(newKoiStandard);
                var result = await _context.SaveChangesAsync();

                if (result != 1)
                {
                    return BadRequest("Failed to create new Koi standard!");
                }

                return Ok(newKoiStandard);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public async Task<IActionResult> HandleUpdateKoiStandard([FromBody] UpdateKoiStandardDTO updateKoiStandardDto)
        {
            if (string.IsNullOrEmpty(updateKoiStandardDto.standard_id))
            {
                return BadRequest("Standard ID is required!");
            }

            try
            {
                var koiStandard = await _context.KoiStandard
                    .FirstOrDefaultAsync(r => r.standard_id == updateKoiStandardDto.standard_id);

                if (koiStandard == null)
                {
                    return BadRequest("Koi standard not found!");
                }

                koiStandard.color_koi = updateKoiStandardDto.color_koi;
                koiStandard.pattern_koi = updateKoiStandardDto.pattern_koi;
                koiStandard.size_koi = updateKoiStandardDto.size_koi;
                koiStandard.age_koi = updateKoiStandardDto.age_koi;
                koiStandard.bodyshape_koi = updateKoiStandardDto.bodyshape_koi;
                koiStandard.variety_koi = updateKoiStandardDto.variety_koi;
                koiStandard.standard_name = updateKoiStandardDto.standard_name;
                koiStandard.gender = updateKoiStandardDto.gender;

                _context.KoiStandard.Update(koiStandard);
                var result = await _context.SaveChangesAsync();

                if (result != 1)
                {
                    return BadRequest("Failed to update Koi standard!");
                }

                return Ok(koiStandard);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        public async Task<IActionResult> HandleDeleteKoiStandard(string standardId)
        {
            try
            {
                var koiStandard = await _context.KoiStandard
                    .FirstOrDefaultAsync(r => r.standard_id == standardId);

                if (koiStandard == null)
                {
                    return BadRequest("Koi standard not found!");
                }

                _context.KoiStandard.Remove(koiStandard);
                var result = await _context.SaveChangesAsync();

                if (result != 1)
                {
                    return BadRequest("Failed to delete Koi standard!");
                }

                return Ok("Koi standard deleted successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public async Task<IActionResult> HandleGetKoiStandard(string standardId)
        {
            try
            {
                var koiStandard = await _context.KoiStandard
                    .Select(standard => new KoiStandardDTO
                    {
                        standard_id = standard.standard_id,
                        color_koi = standard.color_koi,
                        pattern_koi = standard.pattern_koi,
                        size_koi = standard.size_koi,
                        age_koi = standard.age_koi,
                        bodyshape_koi = standard.bodyshape_koi,
                        variety_koi = standard.variety_koi,
                        standard_name = standard.standard_name,
                        gender = standard.gender
                    })
                    .FirstOrDefaultAsync(r => r.standard_id == standardId);

                if (koiStandard == null)
                {
                    return BadRequest("Koi standard not found!");
                }

                return Ok(koiStandard);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
