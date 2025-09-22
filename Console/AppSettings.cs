namespace Console;

public sealed class AppSettings
{
    public required SiloSettings SiloSettings { get; init; }
}

public sealed class SiloSettings
{
    public required string ClusterId { get; init; }
    public required string PgStorageConnection { get; init; }
    public required string PgStorageInvatiant { get; init; }
}