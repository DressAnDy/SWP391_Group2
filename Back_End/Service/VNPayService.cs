using DTO.VNPay;
using Infrastructure.VNPay;
using KoiBet.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace Service.VNPayService;

public interface IVNPayService
{
    public Task<IActionResult> HandleCreateVNPayUrl(HttpContext context, VnPayRequestDTO vnPayRequestDTO, string userId);
    public Task<IActionResult> HandleVNPay(IQueryCollection collection);
}


public class VnPayService : ControllerBase, IVNPayService
{
    private readonly VNPayRepo _vnPayRepo;
    private readonly string? _vnpVersion;
    private readonly string? _vnpCommand;
    private readonly string? _vnpTmnCode;
    private readonly string? _vnpCurrCode;
    private readonly string? _vnpLocale;
    private readonly string? _vnpBaseUrl;
    private readonly string? _vnpHashSecret;
    private readonly ApplicationDbContext _context;

    public VnPayService(IConfiguration config, ApplicationDbContext context)
    {
        _vnPayRepo = new VNPayRepo();
        _vnpVersion = config["VNPay:Version"];
        _vnpCommand = config["VNPay:Command"];
        _vnpTmnCode = config["VNPay:TmnCode"];
        _vnpCurrCode = config["VNPay:CurrCode"];
        _vnpLocale = config["VNPay:Locale"];
        _vnpBaseUrl = config["VNPay:BaseUrl"];
        _vnpHashSecret = config["VNPay:HashSecret"];
        _context = context;
    }

    public async Task<IActionResult> HandleCreateVNPayUrl(HttpContext context, VnPayRequestDTO vnPayRequestDTO, string userId)
    {
        try
        {
            var tick = DateTime.Now.Ticks.ToString();

            var vnpReturnUrl = $"http://localhost:5173/member/payment"; ; 

            _vnPayRepo.AddRequestData("vnp_Version", _vnpVersion);
            _vnPayRepo.AddRequestData("vnp_Command", _vnpCommand);
            _vnPayRepo.AddRequestData("vnp_TmnCode", _vnpTmnCode);
            _vnPayRepo.AddRequestData("vnp_Amount", (vnPayRequestDTO.Amount * 100).ToString());
            _vnPayRepo.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            _vnPayRepo.AddRequestData("vnp_CurrCode", _vnpCurrCode);
            _vnPayRepo.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(context));
            _vnPayRepo.AddRequestData("vnp_Locale", _vnpLocale);
            _vnPayRepo.AddRequestData("vnp_OrderInfo", "Thanh toan don hang:" + userId + "-" + vnPayRequestDTO.Amount);
            _vnPayRepo.AddRequestData("vnp_OrderType", "other");
            _vnPayRepo.AddRequestData("vnp_ReturnUrl", vnpReturnUrl);
            _vnPayRepo.AddRequestData("vnp_TxnRef", userId);

            var paymentUrl = _vnPayRepo.CreateRequestUrl(_vnpBaseUrl, _vnpHashSecret);

            return new OkObjectResult(new { Url = paymentUrl });

        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }
    public async Task<IActionResult> HandleVNPay(IQueryCollection collection)
    {
        try
        {
            var result = new VnPayResponseDTO();

            foreach (var (key, value) in collection)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    _vnPayRepo.AddResponseData(key, value.ToString());
                }
            }

            var vnp_SecureHash = collection.FirstOrDefault(r => r.Key == "vnp_SecureHash").Value;

            bool checkSignature = _vnPayRepo.ValidateSignature(vnp_SecureHash, _vnpHashSecret);
            if (!checkSignature)
            {
                result.Success = false;
                return BadRequest("Payment verification failed due to invalid signature.");
            }

            result.OrderId = _vnPayRepo.GetResponseData("vnp_TxnRef");
            result.PaymentMethod = "VNPay";
            result.OrderDescription = _vnPayRepo.GetResponseData("vnp_OrderInfo");
            result.TransactionId = _vnPayRepo.GetResponseData("vnp_TransactionNo");
            result.VnPayResponseCode = _vnPayRepo.GetResponseData("vnp_ResponseCode");
            result.PaymentId = _vnPayRepo.GetResponseData("vnp_BankTranNo");

            if (result.VnPayResponseCode == "00") // "00" là mã thành công
            {
                result.Success = true;

                var user = await _context.Users.FirstOrDefaultAsync(u => u.user_id == result.OrderId);
                if (user != null)
                {
                    user.Balance += decimal.Parse(_vnPayRepo.GetResponseData("vnp_Amount")) / 100; // Chia 100 để chuyển từ VND về đơn vị thập phân
                    await _context.SaveChangesAsync();
                }
                else
                {
                    result.Success = false;
                    return NotFound("User not found.");
                }
            }
            else
            {
                result.Success = false;
                return BadRequest($"Payment failed with response code: {result.VnPayResponseCode}");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"An error occurred: {ex.Message}");
        }
    }
}



