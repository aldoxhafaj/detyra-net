using EProtokoll.Api.Models;

namespace EProtokoll.Api.Dtos;

public record CreateLetterRequest(
    LetterType Type,
    DocumentClassification Classification,
    string Subject,
    int? ExternalInstitutionId,
    int CreatedByUserId,
    PriorityLevel Priority,
    DateTime? DueDate,
    string? OutgoingChannel,
    DateTime? OutgoingDate,
    string? OutgoingReference);

public record UpdateLetterRequest(
    DocumentClassification Classification,
    string Subject,
    int? ExternalInstitutionId,
    PriorityLevel Priority,
    DateTime? DueDate,
    string? OutgoingChannel,
    DateTime? OutgoingDate,
    string? OutgoingReference);

public record AssignLetterRequest(int AssignedToUserId, string? Note);
public record UpdateStatusRequest(LetterStatus Status);
