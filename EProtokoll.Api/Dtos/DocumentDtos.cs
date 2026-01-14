namespace EProtokoll.Api.Dtos;

public record DocumentResponse(int Id, string FileName, string ContentType, long SizeBytes, bool IsEncrypted, bool IsScanned);
