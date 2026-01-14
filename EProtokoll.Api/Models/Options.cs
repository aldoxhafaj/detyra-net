namespace EProtokoll.Api.Models;

public class JwtOptions
{
    public string Issuer { get; set; } = "eprotokoll";
    public string Audience { get; set; } = "eprotokoll";
    public string Key { get; set; } = "ChangeThisKeyToASecretValue";
    public int AccessTokenMinutes { get; set; } = 60;
    public int RefreshTokenDays { get; set; } = 7;
}

public class StorageOptions
{
    public string RootPath { get; set; } = "Storage";
    public string EncryptionKey { get; set; } = "ChangeThisStorageKeyToASecretValue";
}

public class AlertOptions
{
    public int IntervalSeconds { get; set; } = 300;
}
