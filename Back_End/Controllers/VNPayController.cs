using DTO.VNPay;
using KoiBet.Data;
using KoiBet.DTO.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.AuthService;
using Service.JwtService;
using Service.VNPayService;
using System.Security.Claims;

namespace KoiBet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VNPayController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IVNPayService _vnPayService;

        public VNPayController(IConfiguration config, IVNPayService vnPayService)
        {
            _config = config;
            _vnPayService = vnPayService;
        }

        // POST: auth/login
        [Authorize]
        [HttpPost("Get-Payment-Url")]
        public async Task<IActionResult> GetVNPayUrl([FromBody] VnPayRequestDTO vnPayRequestDTO)
        {
            var currentUser = HttpContext.User;
            var currentUserId = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return await _vnPayService.HandleCreateVNPayUrl(HttpContext, vnPayRequestDTO, currentUserId);
        }

        [AllowAnonymous]
        [HttpPost("Process-Payment")]
        public async Task<IActionResult> ProcessVNPay([FromBody] VnPayProcessDTO vnPayProcessDTO)
        {
            return await _vnPayService.HandleVNPay(vnPayProcessDTO);
        }

        [Authorize]
        [HttpPost("Get-Transactions")]
        public async Task<IActionResult> GetTransaction()
        {
            var currentUser = HttpContext.User;
            var currentUserId = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return await _vnPayService.HandleGetTransactions(currentUserId);
        }
    }
}
