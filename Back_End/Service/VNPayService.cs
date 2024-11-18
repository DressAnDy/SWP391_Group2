using DTO.VNPay;
using Infrastructure.VNPay;
using KoiBet.Data;
using KoiBet.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Transactions;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace Service.VNPayService;

public interface IVNPayService
{
    public Task<IActionResult> HandleCreateVNPayUrl(HttpContext context, VnPayRequestDTO vnPayRequestDTO, string userId);
    public Task<IActionResult> HandleGetTransactions(string currentUserId);
    public Task<IActionResult> HandleVNPay(VnPayProcessDTO vnPayProcessDTO);
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

            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] buffer = new byte[10];
            rng.GetBytes(buffer);

            string chuoi = "";
            for (int i = 0; i < 10; i++)
            {
                char kyTu;
                if (buffer[i] % 2 == 0)
                {
                    // Chọn chữ cái ngẫu nhiên
                    kyTu = (char)('a' + buffer[i] % 26);
                }
                else
                {
                    // Chọn số ngẫu nhiên
                    kyTu = (char)('0' + buffer[i] % 10);
                }
                chuoi += kyTu;
            }

            _vnPayRepo.AddRequestData("vnp_Version", _vnpVersion);
            _vnPayRepo.AddRequestData("vnp_Command", _vnpCommand);
            _vnPayRepo.AddRequestData("vnp_TmnCode", _vnpTmnCode);
            _vnPayRepo.AddRequestData("vnp_Amount", (vnPayRequestDTO.Amount * 100).ToString());
            _vnPayRepo.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            _vnPayRepo.AddRequestData("vnp_CurrCode", _vnpCurrCode);
            _vnPayRepo.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(context));
            _vnPayRepo.AddRequestData("vnp_Locale", _vnpLocale);
            _vnPayRepo.AddRequestData("vnp_OrderInfo", user.Username);
            _vnPayRepo.AddRequestData("vnp_OrderType", "other"); //default value: other
            _vnPayRepo.AddRequestData("vnp_ReturnUrl", vnpReturnUrl);
            _vnPayRepo.AddRequestData("vnp_TxnRef", chuoi);

            var paymentUrl = _vnPayRepo.CreateRequestUrl(_vnpBaseUrl, _vnpHashSecret);

            return new OkObjectResult(new { Url = paymentUrl });

        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }
    public async Task<IActionResult> HandleVNPay(VnPayProcessDTO vnPayProcessDTO)
    {
        try
        {
            var user = _context.Users
                .FirstOrDefault(c => c.Username == vnPayProcessDTO.UserName);

            user.Balance += vnPayProcessDTO.Amount;
            _context.Users.Update(user);

            var random = new Random();
            var id = 0;
            for (int i = 0; i < 8; i++)
            {
                id = id * 10 + random.Next(10);
            }

            var transaction = new Transactions
            {
                transactions_id = id.ToString(),
                users_id = user.user_id,
                Amount = vnPayProcessDTO.Amount,
                transactions_time = DateTime.Now,
                messages = "Topup Balance"
            };

            _context.Transactions.Add(transaction);

            _context.SaveChanges();

            return Ok("Update Balance Success!");
        }
        catch (Exception ex)
        {
            return BadRequest($"An error occurred: {ex.Message}");
        }
    }

    public async Task<IActionResult> HandleGetTransactions(string currentUserId)
    {
        try
        {
            var transactions = _context.Transactions
                .Where(c => c.users_id == currentUserId)
                .ToList();

            return Ok(transactions);
        }
        catch (Exception ex)
        {
            return BadRequest($"An error occurred: {ex.Message}");
        }
    }
}



