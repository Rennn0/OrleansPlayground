using Core.Contracts;
using Core.Models;
using Orleans.Concurrency;

namespace Core.Grains;

[Alias("IWarehouseGrain")]
public interface IWarehouseGrain : IGrainWithIntegerKey
{
    [Alias("CreateWarehouseAsync")]
    ValueTask CreateWarehouseAsync(CreateWarehouse createWarehouse, CancellationToken cancellationToken = default);

    [Alias("UpdateWarehouseAsync")]
    ValueTask UpdateWarehouseAsync(UpdateWarehouse updateWarehouse, CancellationToken cancellationToken = default);

    [Alias("GetWarehouseAsync")]
    [ReadOnly]
    ValueTask<WarehouseModel> GetWarehouseAsync(GetWarehouse getWarehouse,
        CancellationToken cancellationToken = default);
}