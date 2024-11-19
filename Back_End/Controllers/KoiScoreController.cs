using KoiBet.DTO;
using KoiBet.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KoiBet.Controllers;

[Route("api/[controller]")]
[ApiController]
public class KoiScoreController : ControllerBase
{
    private readonly IKoiScoreService _koiScoreService;

    public KoiScoreController(IKoiScoreService koiScoreService)
    {
        _koiScoreService = koiScoreService;
    }

    [HttpGet("Get All KoiScore")]
    public async Task<IActionResult> GettAllKoiScore()
    {
        return await _koiScoreService.HandleGetAllKoiScore();
    }

    [HttpGet("Get KoiScore By KoiId And CompeId")]
    public async Task<IActionResult> GetKoiScoreByKoiIdAndCompeId([FromQuery] SearchKoiScoreDTO searchKoiScoreDTO)
    {
        return await _koiScoreService.HandleGetKoiScoreByKoiIdAndCompeId(searchKoiScoreDTO);
    }

    [Authorize]
    [HttpGet("Get KoiScore By RefereeId")]
    public async Task<IActionResult> GetKoiScoreByRefereeId()
    {
        var currentUser = HttpContext.User;
        var currentUserRole = currentUser.FindFirst(ClaimTypes.Role)?.Value;
        var currentUserId = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Ki?m tra quy?n truy c?p
        if (currentUserRole != "referee")
        {
            return BadRequest(new { message = "Unauthorized!" });
        }

        return await _koiScoreService.HandleGetKoiScoreByRefereeId(currentUserId);
    }

    [Authorize]
    [HttpPost("Create KoiScore")]
    public async Task<IActionResult> CreateKoiScore([FromBody] CreateKoiScoreDTO createKoiScoreDTO)
    {
        var currentUser = HttpContext.User;
        var currentUserRole = currentUser.FindFirst(ClaimTypes.Role)?.Value;
        var currentUserId = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Ki?m tra quy?n truy c?p
        if (currentUserRole != "referee")
        {
            return BadRequest(new { message = "Unauthorized!" });
        }

        return await _koiScoreService.HandleCreateKoiScore(currentUserId, createKoiScoreDTO);
    }

    [Authorize]
    [HttpGet("Update KoiScore")]
    public async Task<IActionResult> UpdateKoiScore([FromBody] UpdateKoiScoreDTO updateKoiScoreDTO)
    {
        var currentUser = HttpContext.User;
        var currentUserRole = currentUser.FindFirst(ClaimTypes.Role)?.Value;
        var currentUserId = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Ki?m tra quy?n truy c?p
        if (currentUserRole != "referee")
        {
            return BadRequest(new { message = "Unauthorized!" });
        }

        return await _koiScoreService.HandleUpdateKoiScore(currentUserId, updateKoiScoreDTO);
    }

    [Authorize]
    [HttpGet("Get KoiScore By UserId")]
    public async Task<IActionResult> GetKoiScoreByUserId()
    {
        var currentUser = HttpContext.User;
        var currentUserId = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return await _koiScoreService.HandleGetKoiScoreByUserId(currentUserId);
    }

    //[HttpDelete("Delete KoiScore")]
    //public async Task<IActionResult> DeleteKoiScore(string koiScoreId)
    //{
    //    return await _koiScoreService.HandleDeleteKoiScore(koiScoreId);
    //}
}