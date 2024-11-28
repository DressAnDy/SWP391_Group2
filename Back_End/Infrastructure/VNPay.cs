using System.Globalization;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.VNPay;

public class VNPayRepo
{
    private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
    private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

    public VNPayRepo()
    {

    }

    public void AddRequestData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _requestData.Add(key, value);
        }
    }

    public string AddResponseData(string key, string value)
    {
        var data = new StringBuilder();

        // Xóa các tham số vnp_SecureHashType và vnp_SecureHash khỏi dữ liệu phản hồi
        if (_responseData.ContainsKey("vnp_SecureHashType"))
        {
            _responseData.Remove("vnp_SecureHashType");
        }

        if (_responseData.ContainsKey("vnp_SecureHash"))
        {
            _responseData.Remove("vnp_SecureHash");
        }

        // Tạo chuỗi tham số đã mã hóa URL (đảm bảo thứ tự đúng)
        foreach (var entry in _responseData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
        {
            data.Append(WebUtility.UrlEncode(entry.Key) + "=" + WebUtility.UrlEncode(entry.Value) + "&");
        }

        // Xóa ký tự '&' cuối cùng nếu có
        if (data.Length > 0)
        {
            data.Remove(data.Length - 1, 1);
        }

        // Trả về chuỗi kết quả
        return data.ToString();
    }

    public string GetResponseData(string key)
    {
        return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
    }

    #region Request
    public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
    {
        var data = new StringBuilder();

        // Loops through the request data and adds each non-empty parameter to the query string.
        foreach (var (key, value) in _requestData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
        {
            data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
        }

        var querystring = data.ToString();
        baseUrl += "?" + querystring;

        // Remove the last '&' from the query string.
        var signData = querystring.TrimEnd('&');

        // Calculate the secure hash using HMACSHA512.
        var vnpSecureHash = Utils.HmacSHA512(vnpHashSecret, signData);

        // Add the secure hash to the base URL.
        baseUrl += "vnp_SecureHash=" + vnpSecureHash;

        return baseUrl;
    }
    #endregion

    #region Response process
    public bool ValidateSignature(string inputHash, string secretKey)
    {
        var rspRaw = GetResponseData();
        var myChecksum = Utils.HmacSHA512(secretKey, rspRaw);
        return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
    }

    private string GetResponseData()
    {
        var data = new StringBuilder();
        if (_responseData.ContainsKey("vnp_SecureHashType"))
        {
            _responseData.Remove("vnp_SecureHashType");
        }

        if (_responseData.ContainsKey("vnp_SecureHash"))
        {
            _responseData.Remove("vnp_SecureHash");
        }

        foreach (var (key, value) in _responseData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
        {
            data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
        }

        //remove last '&'
        if (data.Length > 0)
        {
            data.Remove(data.Length - 1, 1);
        }

        return data.ToString();
    }
    #endregion
}

public class Utils
{
    public static string HmacSHA512(string key, string inputData)
    {
        var hash = new StringBuilder();
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(inputData);
        using (var hmac = new HMACSHA512(keyBytes))
        {
            var hashValue = hmac.ComputeHash(inputBytes);
            foreach (var theByte in hashValue)
            {
                hash.Append(theByte.ToString("x2"));
            }
        }

        return hash.ToString();
    }


    // có chế biến cho .NET Core MVC
    public static string GetIpAddress(HttpContext context)
    {
        var ipAddress = string.Empty;
        try
        {
            var remoteIpAddress = context.Connection.RemoteIpAddress;

            if (remoteIpAddress != null)
            {
                if (remoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    remoteIpAddress = Dns.GetHostEntry(remoteIpAddress).AddressList
                        .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                }

                if (remoteIpAddress != null) ipAddress = remoteIpAddress.ToString();

                return ipAddress;
            }
        }
        catch (Exception ex)
        {
            return "Invalid IP:" + ex.Message;
        }

        return "127.0.0.1";
    }
}

public class VnPayCompare : IComparer<string>
{
    public int Compare(string x, string y)
    {
        if (x == y) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        var vnpCompare = CompareInfo.GetCompareInfo("en-US");
        return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
    }
}
