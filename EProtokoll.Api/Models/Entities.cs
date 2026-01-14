namespace EProtokoll.Api.Models;

public class AppUser
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "Employee";
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ExternalInstitution
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ExternalId { get; set; }
    public string? Address { get; set; }
    public string? Contact { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ProtocolBook
{
    public int Id { get; set; }
    public int Year { get; set; }
    public bool IsOpen { get; set; }
    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }
}

public class ProtocolCounter
{
    public int Id { get; set; }
    public int Year { get; set; }
    public int CurrentValue { get; set; }
}

public class Letter
{
    public int Id { get; set; }
    public LetterType Type { get; set; }
    public DocumentClassification Classification { get; set; }
    public string ProtocolNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public int? ExternalInstitutionId { get; set; }
    public ExternalInstitution? ExternalInstitution { get; set; }
    public int CreatedByUserId { get; set; }
    public AppUser? CreatedByUser { get; set; }
    public int? AssignedToUserId { get; set; }
    public AppUser? AssignedToUser { get; set; }
    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
    public DateTime? DueDate { get; set; }
    public string? OutgoingChannel { get; set; }
    public DateTime? OutgoingDate { get; set; }
    public string? OutgoingReference { get; set; }
    public LetterStatus Status { get; set; } = LetterStatus.New;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<LetterAccess> AccessUsers { get; set; } = new List<LetterAccess>();
    public ICollection<LetterDepartmentAccess> AccessDepartments { get; set; } = new List<LetterDepartmentAccess>();
}

public class Document
{
    public int Id { get; set; }
    public int LetterId { get; set; }
    public Letter? Letter { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string HashSha256 { get; set; } = string.Empty;
    public StorageProvider StorageProvider { get; set; } = StorageProvider.FileSystem;
    public string StorageKey { get; set; } = string.Empty;
    public bool IsEncrypted { get; set; }
    public bool IsScanned { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Assignment
{
    public int Id { get; set; }
    public int LetterId { get; set; }
    public Letter? Letter { get; set; }
    public int AssignedByUserId { get; set; }
    public int AssignedToUserId { get; set; }
    public string? Note { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}

public class ResponseEntry
{
    public int Id { get; set; }
    public int LetterId { get; set; }
    public Letter? Letter { get; set; }
    public int UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class AuditLog
{
    public int Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Details { get; set; }
}

public class LetterAccess
{
    public int Id { get; set; }
    public int LetterId { get; set; }
    public Letter? Letter { get; set; }
    public int UserId { get; set; }
    public AppUser? User { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class LetterDepartmentAccess
{
    public int Id { get; set; }
    public int LetterId { get; set; }
    public Letter? Letter { get; set; }
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class DocumentHistory
{
    public int Id { get; set; }
    public int LetterId { get; set; }
    public Letter? Letter { get; set; }
    public string Action { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? LetterId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class RefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRevoked { get; set; }
}
