using System.ComponentModel.DataAnnotations;

namespace Console;

public sealed class AppSettings
{
    [Required] public required SiloSettings SiloSettings { get; init; }
}

public sealed class SiloSettings
{
    [Required] [MinLength(5)] public required string ClusterId { get; init; }
    [Required] [MinLength(16)] public required string PgStorageConnection { get; init; }
    public string AzureBlobConnection { get; init; }
    [Required] [MinLength(1)] public required string PgStorageInvatiant { get; init; }
}