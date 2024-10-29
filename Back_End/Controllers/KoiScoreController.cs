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

    //[HttpGet("Get Competition By Id")]
    //public async Task<IActionResult> GetKoiScoreById(string koiScoreId)
    //{
    //    return await _koiScoreService.HandleGetKoiScoreById(koiScoreId);
    //}

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

    //[Authorize]
    //[HttpGet("Update KoiScore")]
    //public async Task<IActionResult> UpdateKoiScore([FromBody] UpdateKoiScoreDTO updateKoiScoreDTO)
    //{
    //    var currentUser = HttpContext.User;
    //    var currentUserRole = currentUser.FindFirst(ClaimTypes.Role)?.Value;
    //    var currentUserId = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    //    // Ki?m tra quy?n truy c?p
    //    if (currentUserRole != "referee")
    //    {
    //        return BadRequest(new { message = "Unauthorized!" });
    //    }

    //    return await _koiScoreService.HandleUpdateKoiScore(currentUserId, updateKoiScoreDTO);
    //}

    //[HttpDelete("Delete KoiScore")]
    //public async Task<IActionResult> DeleteKoiScore(string koiScoreId)
    //{
    //    return await _koiScoreService.HandleDeleteKoiScore(koiScoreId);
    //}
}