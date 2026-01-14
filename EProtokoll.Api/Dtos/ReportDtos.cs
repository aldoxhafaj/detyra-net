namespace EProtokoll.Api.Dtos;

public record SummaryReport(int Total, int Incoming, int Outgoing, int Internal, int PublicCount, int RestrictedCount, int SecretCount);
public record OverdueReport(int LetterId, string ProtocolNumber, string Subject, int DaysOverdue, int? AssignedToUserId);
public record UserReport(int UserId, int Count);
public record TrackingReport(int LetterId, string ProtocolNumber, string Subject, string LastAction, DateTime LastActionAt, int DaysOverdue);
public record PriorityReport(string Priority, int Count);
public record StatusReport(string Status, int Count);
public record DepartmentReport(int? DepartmentId, int Count);
