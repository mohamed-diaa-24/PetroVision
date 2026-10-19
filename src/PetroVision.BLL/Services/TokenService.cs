using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PetroVision.BLL.Interfaces;
using PetroVision.BLL.Settings;
using PetroVision.DAL.Entities;

namespace PetroVision.BLL.Services;

public class TokenService : ITokenService
{
    private readonly JwtSettings _settings;

    public TokenService(IOptions<JwtSettings> settings) => _settings = settings.Value;

    public (string Token, DateTime ExpiresAt) CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new("sub", user.Id.ToString()),
            new("name", user.FullName),
            new("email", user.Email),
            new("role", user.Role.ToString())
        };

        if (user.DepartmentId.HasValue)
            claims.Add(new Claim("department", user.DepartmentId.Value.ToString()));

        claims.AddRange(user.Permissions.Select(p => new Claim("permission", p.Permission.ToString())));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
