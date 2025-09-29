using Orleans.Concurrency;

namespace Silo.Warehouse.Grains;

public class WarehouseGrain : ApplicationGrain<WarehouseModel>, IWarehouseGrain, IRemindable
{
    private readonly IOptionsMonitor<AppSettings> _appSettings;
    private readonly ILogger<WarehouseGrain> _logger;

    public WarehouseGrain(IOptionsMonitor<AppSettings> appSettings, ILoggerFactory loggerFactory,
        IPersistentStateFactory stateFactory) : base(loggerFactory,
        stateFactory, appSettings.CurrentValue.SiloSettings.WarehouseState,
        appSettings.CurrentValue.SiloSettings.AzureStorage)
    {
        _logger = loggerFactory.CreateLogger<WarehouseGrain>();
        _appSettings = appSettings;

        this.RegisterOrUpdateReminder("reminder1", TimeSpan.Zero, TimeSpan.FromMinutes(1));
        this.RegisterOrUpdateReminder("reminder2", TimeSpan.FromSeconds(6), TimeSpan.FromSeconds(61));
    }

    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        _logger.LogDebug("status {@status}, id {@IdentityString}, state {@ApplicationState}", status, IdentityString,
            Stringify(ApplicationState));

        switch (reminderName)
        {
            case "reminder1":
                _logger.LogCritical(reminderName);
                break;
            case "reminder2":
                _logger.LogError(reminderName);
                await this.UnregisterReminder(await this.GetReminder("reminder2") ?? throw new ApplicationException());
                break;
        }
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

    [ReadOnly]
    public ValueTask<WarehouseModel> GetWarehouseAsync(GetWarehouse getWarehouse,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(new EventId(_appSettings.CurrentValue.MethodCallId), "Get Warehouse {@GetWarehouse}",
            getWarehouse);
        return ValueTask.FromResult(ApplicationState);
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning(new EventId(_appSettings.CurrentValue.ActivationId), "WarehouseGrain OnActivateAsync");
        return base.OnActivateAsync(cancellationToken);
    }

    public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        _logger.LogWarning(new EventId(_appSettings.CurrentValue.DeactivationId), "WarehouseGrain OnDeactivateAsync");
        return base.OnDeactivateAsync(reason, cancellationToken);
    }
}