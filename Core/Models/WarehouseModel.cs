namespace Core.Models;

[GenerateSerializer]
[Alias("WarehouseModel")]
public class WarehouseModel
{
    [Id(0)] public object? Id { get; set; }

    [Id(1)] public string? Location { get; set; }

    [Id(2)] public string? Owner { get; set; }

    [Id(3)] public long? Capacity { get; set; }

    [Id(4)] public string? IdentityString { get; set; }
}

//{"$id":"1","$type":"Core.Models.WarehouseModel, Core","Id":10,"Location":"geo","Owner":"yavela","Capacity":0,"IdentityString":"warehouse/A"}