using System.ComponentModel.DataAnnotations;

namespace Silo.Warehouse;

public sealed class AppSettings
{
    public int ActivationId { get; init; }
    public int DeactivationId { get; init; }
    public int MethodCallId { get; init; }
    public int InnerTraceId { get; init; }

    [Required] [MinLength(3)] public required string Environment { get; init; }

    [Required] public required SiloSettings SiloSettings { get; init; }
}

public sealed class SiloSettings
{
    public bool UseDashboard { get; init; }
    public int PersistSecondsDelay { get; init; }
    public int PersistSecondsPeriod { get; init; }
    [Required] [Range(1024, 65535)] public int DashboardPort { get; init; }
    [Required] [Range(1024, 65535)] public int SiloPort { get; init; }
    [Required] [Range(1024, 65535)] public int GatewayPort { get; init; }
    [Required] [MinLength(1)] public required string ClusterId { get; init; }
    [Required] [MinLength(1)] public required string ServiceId { get; init; }
    [Required] [MinLength(1)] public required string PgStorageConnection { get; init; }
    [Required] [MinLength(1)] public required string PgStorageInvatiant { get; init; }

    [Required] [MinLength(1)] public required string WarehouseState { get; init; }

    [Required] [MinLength(1)] public required string WarehouseStorage { get; init; }
}