namespace EProtokoll.Desktop;

public record TokenResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);

public record LetterDto
{
    public int Id { get; init; }
    public string ProtocolNumber { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Classification { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime? DueDate { get; init; }
    public string? OutgoingChannel { get; init; }
    public DateTime? OutgoingDate { get; init; }
    public string? OutgoingReference { get; init; }
}

public record InstitutionDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? ExternalId { get; init; }
    public string? Address { get; init; }
    public string? Contact { get; init; }
}

public record UserDto
{
    public int Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public int? DepartmentId { get; init; }
    public bool IsActive { get; init; }
}

public record DocumentDto
{
    public int Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
    public bool IsEncrypted { get; init; }
    public bool IsScanned { get; init; }
}

public record NotificationDto
{
    public int Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public bool IsRead { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record ResponseDto
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public record AccessUserDto
{
    public int UserId { get; init; }
}

public record DepartmentDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
}

public record AccessDepartmentDto
{
    public int DepartmentId { get; init; }
}
