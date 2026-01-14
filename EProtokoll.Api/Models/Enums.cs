namespace EProtokoll.Api.Models;

public enum LetterType
{
    Incoming = 0,
    Outgoing = 1,
    Internal = 2
}

public enum DocumentClassification
{
    Public = 0,
    Restricted = 1,
    Secret = 2
}

public enum LetterStatus
{
    New = 0,
    InProgress = 1,
    Closed = 2
}

public enum PriorityLevel
{
    Low = 0,
    Medium = 1,
    High = 2,
    Urgent = 3
}

public enum StorageProvider
{
    FileSystem = 0
}
