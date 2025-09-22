using Orleans.Concurrency;

namespace Silo.Warehouse.Grains;

[ResourceOptimizedPlacement]
[KeepAlive]
public class WarehouseGrain : Grain, IWarehouseGrain
{
    private readonly IOptionsMonitor<AppSettings> _appSettings;
    private readonly Channel<bool> _flushChannel;
    private readonly ILogger<WarehouseGrain> _logger;
    private readonly IPersistentState<WarehouseModel> _warehouseState;

    public WarehouseGrain(
        ILogger<WarehouseGrain> logger,
        IOptionsMonitor<AppSettings> appSettings,
        IPersistentStateFactory stateFactory
    )
    {
        _logger = logger;
        _appSettings = appSettings;
        _warehouseState = stateFactory.Create<WarehouseModel>(GrainContext,
            new PersistentStateAttribute(_appSettings.CurrentValue.SiloSettings.WarehouseState,
                _appSettings.CurrentValue.SiloSettings.WarehouseStorage));
        _flushChannel = Channel.CreateBounded<bool>(1);
    }

    [OneWay]
    public ValueTask CreateWarehouseAsync(CreateWarehouse createWarehouse,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(new EventId(_appSettings.CurrentValue.MethodCallId),
            "Creating warehouse {@CreateWarehouse}", createWarehouse);
        Mutate(() => new WarehouseModel
            {
                Owner = createWarehouse.Owner,
                Location = createWarehouse.Location,
                Capacity = createWarehouse.Capacity,
                Id = this.GetPrimaryKeyLong(),
                IdentityString = IdentityString
            }
        );
        return ValueTask.CompletedTask;
    }

    [OneWay]
    public ValueTask UpdateWarehouseAsync(UpdateWarehouse updateWarehouse,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(new EventId(_appSettings.CurrentValue.MethodCallId),
            "Updating warehouse {@UpdateWarehouse}", updateWarehouse);
        Mutate(model =>
        {
            model.Owner = updateWarehouse.Owner ?? model.Owner;
            model.Location = updateWarehouse.Location ?? model.Location;
            model.Capacity = updateWarehouse.Capacity ?? model.Capacity;
        });

        return ValueTask.CompletedTask;
    }

    public ValueTask<WarehouseModel> GetWarehouseAsync(GetWarehouse getWarehouse,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(new EventId(_appSettings.CurrentValue.MethodCallId), "Get Warehouse {@GetWarehouse}",
            getWarehouse);
        return ValueTask.FromResult(_warehouseState.State);
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning(new EventId(_appSettings.CurrentValue.ActivationId), "WarehouseGrain OnActivateAsync");
        _ = FlushChannel();
        return base.OnActivateAsync(cancellationToken);
    }

    public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        _logger.LogWarning(new EventId(_appSettings.CurrentValue.DeactivationId), "WarehouseGrain OnDeactivateAsync");
        _flushChannel.Writer.TryComplete();
        await _warehouseState.WriteStateAsync(cancellationToken);
        await base.OnDeactivateAsync(reason, cancellationToken);
    }

    private void Mutate(Action<WarehouseModel> mutation, [CallerMemberName] string caller = "",
        [CallerLineNumber] int line = 0)
    {
        _logger.LogTrace(new EventId(_appSettings.CurrentValue.InnerTraceId), "caller {caller}, line {line}",
            caller, line);

        mutation(_warehouseState.State);
        _flushChannel.Writer.TryWrite(true);
    }

    private void Mutate(Func<WarehouseModel> mutation, [CallerMemberName] string caller = "",
        [CallerLineNumber] int line = 0)
    {
        _logger.LogTrace(new EventId(_appSettings.CurrentValue.InnerTraceId), "caller {caller}, line {line}",
            caller, line);

        _warehouseState.State = mutation();
        _flushChannel.Writer.TryWrite(true);
    }

    private async Task FlushChannel()
    {
        await foreach (bool _ in _flushChannel.Reader.ReadAllAsync())
            await _warehouseState.WriteStateAsync(CancellationToken.None);
    }
}