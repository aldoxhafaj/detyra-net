namespace EProtokoll.Api.Dtos;

public record NotificationResponse(int Id, string Type, string Message, bool IsRead, DateTime CreatedAt, int? LetterId);
