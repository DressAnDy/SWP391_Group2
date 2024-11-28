namespace Service.JwtService;

using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Service.Payload;
using System.Collections.Generic;
using System.Linq;
using KoiBet.DTO.User;
using System.Security.Cryptography;

public class JwtService
{
    private readonly string DEFAULT_SECRET = "PDv7DrqznYL6nv7DrqzjnQYO9JxIsWdcjnQYL6nu0f";
    private readonly byte[] _key;
    private readonly JwtSecurityTokenHandler _handler;
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        var SecretKey = Environment.GetEnvironmentVariable("JWT_SECRET") ?? DEFAULT_SECRET;
        _key = Encoding.ASCII.GetBytes(SecretKey);
        _handler = new JwtSecurityTokenHandler();
        _config = config;

    }

    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var tokeOptions = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: signinCredentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
        return tokenString;
    }

    public string GenerateSecurityToken(PayloadDTO payloadDTO)
    {
        var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET") ?? DEFAULT_SECRET);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
             {
                 new ("userID", payloadDTO.UserID),
                 new ("email", payloadDTO.Email),
                 new ("isAdmin", payloadDTO.IsAdmin.ToString()),
             }),
            Expires = DateTime.UtcNow.AddMinutes(30),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = payloadDTO.UserID,
        };

        var token = _handler.CreateToken(tokenDescriptor);

        return _handler.WriteToken(token);

    }

    public Payload? ValidateToken(string token)
    {

        _handler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(_key),
            ValidateIssuer = false,
            ValidateAudience = false,
            // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
            ClockSkew = TimeSpan.FromHours(2)
        }, out SecurityToken validatedToken);

        var result = (JwtSecurityToken)validatedToken;

        var payload = new Payload()
        {
            UserId = result.Issuer,
            Email = result.Claims.First(x => x.Type == "email").Value,
            IsAdmin = bool.Parse(result.Claims.First(x => x.Type == "isAdmin").Value)
        };

        return payload;
    }

    public string CreateNewGuid()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new System.Random(); // Define the random instance here
        StringBuilder result = new StringBuilder();

        for (int i = 0; i < 20; i++)
        {
            result.Append(chars[random.Next(chars.Length)]);
        }

        return result.ToString();
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"])),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Token không hợp lệ");
            }
            return principal;
        }
        catch (Exception ex)
        {
            throw new SecurityTokenException($"Lỗi khi xác thực token: {ex.Message}");
        }
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}