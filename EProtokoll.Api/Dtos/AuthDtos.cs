namespace EProtokoll.Api.Dtos;

public record LoginRequest(string UserName, string Password);
public record RefreshRequest(string RefreshToken);
public record TokenResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);

public record CreateUserRequest(string UserName, string FullName, string Role, string Password, int? DepartmentId);
public record UpdateUserRequest(string FullName, string Role, bool IsActive, int? DepartmentId);
