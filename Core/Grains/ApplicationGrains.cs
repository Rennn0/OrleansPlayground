using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.Placement;

namespace Core.Grains;

[PreferLocalPlacement]
[StatelessWorker]
[Reentrant]
public abstract class StatelessApplicationGrain : Grain
{
}

/// <summary>
///     single state grain
/// </summary>
/// <typeparam name="TState">state type</typeparam>
[ResourceOptimizedPlacement]
public abstract class ApplicationGrain<TState> : Grain where TState : class
{
    private readonly Channel<bool> _flushChannel;

    private readonly ILogger<ApplicationGrain<TState>> _logger;
    private readonly IPersistentState<TState> _persistentState;

    protected ApplicationGrain(ILoggerFactory loggerFactory, IPersistentStateFactory stateFactory, string state,
        string storage
    )
    {
        _logger = loggerFactory.CreateLogger<ApplicationGrain<TState>>();
        _flushChannel = Channel.CreateBounded<bool>(1);
        _persistentState = stateFactory.Create<TState>(GrainContext, new PersistentStateAttribute(state, storage));
    }

    protected TState ApplicationState => _persistentState.State;

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _ = FlushBehavior();
        return base.OnActivateAsync(cancellationToken);
    }

    public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        _flushChannel.Writer.TryComplete();
        return base.OnDeactivateAsync(reason, cancellationToken);
    }

    /// <summary>
    ///     use this to modify state
    /// </summary>
    /// <param name="mutator"></param>
    protected virtual bool Mutate(Action<TState> mutator)
    {
        _logger.LogInformation(new EventId(-1), "Mutating application state");
        _logger.LogTrace("before {@State}", Stringify(_persistentState.State));
        mutator(_persistentState.State);
        _logger.LogTrace("after {@State}", Stringify(_persistentState.State));
        return AddFlush();
    }

    /// <summary>
    ///     use this to instantiate state
    /// </summary>
    /// <param name="mutator"></param>
    protected virtual bool Mutate(Func<TState> mutator)
    {
        _logger.LogInformation(new EventId(-2), "Creating application state");
        _logger.LogTrace("before {@State}", Stringify(_persistentState.State));
        _persistentState.State = mutator();
        _logger.LogTrace("after {@State}", Stringify(_persistentState.State));
        return AddFlush();
    }

    protected bool AddFlush()
    {
        return _flushChannel.Writer.TryWrite(true);
    }

    protected virtual Task OnFlushAsync()
    {
        _logger.LogInformation(new EventId(-3), "Flushing application state");
        _logger.LogTrace("state obj {Sttate}", Stringify(_persistentState.State));
        return _persistentState.WriteStateAsync();
    }

    private async Task FlushBehavior()
    {
        await foreach (bool _ in _flushChannel.Reader.ReadAllAsync()) await OnFlushAsync();
    }

    protected string Stringify(object obj)
    {
        return JsonSerializer.Serialize(obj,
            new JsonSerializerOptions { WriteIndented = true, ReferenceHandler = ReferenceHandler.IgnoreCycles });
    }

    protected Task ClearApplicationStateAsync()
    {
        return _persistentState.ClearStateAsync();
    }
}

/// <summary>
///     double state grain
/// </summary>
/// <typeparam name="TState1">first state type</typeparam>
/// <typeparam name="TState2">second state type</typeparam>
[ResourceOptimizedPlacement]
public abstract class ApplicationGrain<TState1, TState2> : ApplicationGrain<TState1>
    where TState1 : class where TState2 : class
{
    private readonly IPersistentState<TState2> _persistentState;

    protected ApplicationGrain(ILoggerFactory loggerFactory, IPersistentStateFactory stateFactory, string state,
        string storage) : base(loggerFactory, stateFactory, state,
        storage)
    {
        _persistentState =
            stateFactory.Create<TState2>(GrainContext, new PersistentStateAttribute(state, storage));
    }

    protected ApplicationGrain(ILoggerFactory loggerFactory, IPersistentStateFactory stateFactory, string state1,
        string storage1, string state2,
        string storage2) : base(loggerFactory, stateFactory, state1,
        storage1)
    {
        _persistentState =
            stateFactory.Create<TState2>(GrainContext, new PersistentStateAttribute(state2, storage2));
    }

    protected TState2 ApplicationState2 => _persistentState.State;

    /// <summary>
    ///     use this to modify state
    /// </summary>
    /// <param name="mutator"></param>
    protected virtual bool Mutate(Action<TState2> mutator)
    {
        mutator(_persistentState.State);
        return AddFlush();
    }

    /// <summary>
    ///     use this to instantiate state
    /// </summary>
    /// <param name="mutator"></param>
    protected virtual bool Mutate(Func<TState2> mutator)
    {
        _persistentState.State = mutator();
        return AddFlush();
    }

    protected override Task OnFlushAsync()
    {
        return Task.WhenAll(_persistentState.WriteStateAsync(), base.OnFlushAsync());
    }
}