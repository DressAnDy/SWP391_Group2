using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Encodings.Web;

public class CustomJwtBearerHandler : AuthenticationHandler<JwtBearerOptions>
{
    private readonly IConfiguration _configuration;

    public CustomJwtBearerHandler(
        IOptionsMonitor<JwtBearerOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IConfiguration configuration)
        : base(options, logger, encoder, clock)
    {
        _configuration = configuration;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (string.IsNullOrEmpty(token))
        {
            return AuthenticateResult.NoResult();
        }

        // Thêm "Bearer " vào đầu token nếu nó không có
        if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = token.Trim();
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            if (jwtToken == null || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return AuthenticateResult.Fail("Invalid token");
            }

            return AuthenticateResult.Success(new AuthenticationTicket(principal, JwtBearerDefaults.AuthenticationScheme));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error authenticating token");
            return AuthenticateResult.Fail("Invalid token");
        }
    }
}

public class CustomJwtBearerMiddleware
{
    private readonly RequestDelegate _next;

    public CustomJwtBearerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, CustomJwtBearerHandler handler)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(token))
        {
            if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                context.Request.Headers["Authorization"] = "Bearer " + token;
            }
        }

        await _next(context);
    }
}

public static class CustomJwtBearerMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomJwtBearer(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CustomJwtBearerMiddleware>();
    }
}