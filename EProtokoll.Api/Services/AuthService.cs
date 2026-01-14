using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EProtokoll.Api.Data;
using EProtokoll.Api.Dtos;
using EProtokoll.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EProtokoll.Api.Services;

public interface IAuthService
{
    Task<TokenResponse?> LoginAsync(LoginRequest request);
    Task<TokenResponse?> RefreshAsync(RefreshRequest request);
    Task<AppUser> CreateUserAsync(CreateUserRequest request);
    Task<AppUser?> UpdateUserAsync(int id, UpdateUserRequest request);
    Task<AppUser?> GetUserAsync(int id);
    Task<List<AppUser>> GetUsersAsync();
    Task<bool> DeleteUserAsync(int id);
    Task EnsureSeedAdminAsync();
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly JwtOptions _jwt;

    public AuthService(AppDbContext db, IOptions<JwtOptions> jwt)
    {
        _db = db;
        _jwt = jwt.Value;
    }

    public async Task<TokenResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users.SingleOrDefaultAsync(x => x.UserName == request.UserName && x.IsActive);
        if (user == null)
        {
            return null;
        }

        if (!VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            return null;
        }

        return await GenerateTokensAsync(user);
    }

    public async Task<TokenResponse?> RefreshAsync(RefreshRequest request)
    {
        var refresh = await _db.RefreshTokens.SingleOrDefaultAsync(x => x.Token == request.RefreshToken);
        if (refresh == null || refresh.IsRevoked || refresh.ExpiresAt < DateTime.UtcNow)
        {
            return null;
        }

        var user = await _db.Users.FindAsync(refresh.UserId);
        if (user == null || !user.IsActive)
        {
            return null;
        }

        refresh.IsRevoked = true;
        await _db.SaveChangesAsync();
        return await GenerateTokensAsync(user);
    }

    public async Task<AppUser> CreateUserAsync(CreateUserRequest request)
    {
        var existing = await _db.Users.AnyAsync(x => x.UserName == request.UserName);
        if (existing)
        {
            throw new InvalidOperationException("User already exists");
        }

        CreatePasswordHash(request.Password, out var hash, out var salt);
        var user = new AppUser
        {
            UserName = request.UserName,
            FullName = request.FullName,
            Role = request.Role,
            DepartmentId = request.DepartmentId,
            PasswordHash = hash,
            PasswordSalt = salt
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<AppUser?> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null)
        {
            return null;
        }

        user.FullName = request.FullName;
        user.Role = request.Role;
        user.IsActive = request.IsActive;
        user.DepartmentId = request.DepartmentId;
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<AppUser?> GetUserAsync(int id)
    {
        return await _db.Users.FindAsync(id);
    }

    public async Task<List<AppUser>> GetUsersAsync()
    {
        return await _db.Users.AsNoTracking().ToListAsync();
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null)
        {
            return false;
        }

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task EnsureSeedAdminAsync()
    {
        if (await _db.Users.AnyAsync())
        {
            return;
        }

        CreatePasswordHash("Admin123!", out var hash, out var salt);
        _db.Users.Add(new AppUser
        {
            UserName = "admin",
            FullName = "System Administrator",
            Role = "Administrator",
            PasswordHash = hash,
            PasswordSalt = salt
        });

        await _db.SaveChangesAsync();
    }

    private async Task<TokenResponse> GenerateTokensAsync(AppUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays)
        });
        await _db.SaveChangesAsync();
        return new TokenResponse(accessToken, refreshToken, expires);
    }

    private static void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
    {
        using var hmac = new HMACSHA256();
        salt = hmac.Key;
        hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    private static bool VerifyPassword(string password, byte[] hash, byte[] salt)
    {
        using var hmac = new HMACSHA256(salt);
        var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return computed.SequenceEqual(hash);
    }
}
