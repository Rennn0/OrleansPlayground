namespace Core.Contracts;

[GenerateSerializer]
[Alias("CreateWarehouse")]
public record CreateWarehouse(string Location, string Owner, long Capacity);

[GenerateSerializer]
[Alias("UpdateWarehouse")]
public record UpdateWarehouse(string? Location, string? Owner, long? Capacity);

[GenerateSerializer]
public enum WhSearchPreferences
{
    ByLocation,
    ByOwner,
    ByCapacity
}

[GenerateSerializer]
[Alias("GetWarehouse")]
public record GetWarehouse(
    object? Id = null,
    string? Location = null,
    string? Owner = null,
    long? Capacity = null,
    WhSearchPreferences? WhSearchPreferences = null);