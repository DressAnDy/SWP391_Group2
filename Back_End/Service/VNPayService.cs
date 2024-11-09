using DTO.VNPay;
using Infrastructure.VNPay;
using KoiBet.Data;
using Microsoft.AspNetCore.Mvc;
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

            var user = _context.Users.FirstOrDefault(c => c.user_id == userId);

            var vnpReturnUrl = $"http://localhost:5173/member/payment"; ; 

            _vnPayRepo.AddRequestData("vnp_Version", _vnpVersion);
            _vnPayRepo.AddRequestData("vnp_Command", _vnpCommand);
            _vnPayRepo.AddRequestData("vnp_TmnCode", _vnpTmnCode);
            _vnPayRepo.AddRequestData("vnp_Amount", (vnPayRequestDTO.Amount * 100).ToString());
            _vnPayRepo.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            _vnPayRepo.AddRequestData("vnp_CurrCode", _vnpCurrCode);
            _vnPayRepo.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(context));
            _vnPayRepo.AddRequestData("vnp_Locale", _vnpLocale);
            _vnPayRepo.AddRequestData("vnp_OrderInfo", "Thanh toan don hang:" + user.Username);
            _vnPayRepo.AddRequestData("vnp_OrderType", "other"); //default value: other
            _vnPayRepo.AddRequestData("vnp_ReturnUrl", vnpReturnUrl);
            _vnPayRepo.AddRequestData("vnp_TxnRef", user.Username + (vnPayRequestDTO.Amount * 100).ToString());

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
            //var vnp_SecureHash;
            foreach (var (key, value) in collection)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    _vnPayRepo.AddResponseData(key, value.ToString());
                }
                var vnp_SecureHash = collection.FirstOrDefault(r => r.Key == "vnp_SecureHash").Value;

                bool checkSignature = _vnPayRepo.ValidateSignature(vnp_SecureHash, _vnpHashSecret);
                if (!checkSignature)
                {
                    result.Success = false;
                    return BadRequest("Payment not successfull!");
                }
            }
            result.Success = true; 
            result.OrderId = _vnPayRepo.GetResponseData("vnp_TxnRef");
            result.PaymentMethod = "VNPay";
            result.OrderDescription = _vnPayRepo.GetResponseData("vnp_OrderInfo");
            result.OrderId = _vnPayRepo.GetResponseData("vnp_TxnRef");
            result.TransactionId = _vnPayRepo.GetResponseData("vnp_TransactionNo");
            result.VnPayResponseCode = _vnPayRepo.GetResponseData("vnp_ResponseCode");

            var user = _context.Users
                .FirstOrDefault(u => u.user_id == result.OrderId);

            user.Balance = user.Balance + Decimal.Parse(_vnPayRepo.GetResponseData("vnp_Amount"));

            await _context.SaveChangesAsync();

            return Ok(result);
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }
}



