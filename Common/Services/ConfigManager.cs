namespace StardewMods.Common.Services;

using StardewMods.Common.Interfaces;
using StardewMods.Common.Models.Events;

/// <summary>Service for managing the mod configuration file.</summary>
/// <typeparam name="TConfig">The mod configuration type.</typeparam>
internal class ConfigManager<TConfig>
    where TConfig : class, new()
{
    private readonly IDataHelper dataHelper;
    private readonly IEventPublisher eventPublisher;
    private readonly IModHelper modHelper;

    private bool initialized;

    /// <summary>Initializes a new instance of the <see cref="ConfigManager{TConfig}" /> class.</summary>
    /// <param name="dataHelper">Dependency used for storing and retrieving data.</param>
    /// <param name="eventPublisher">Dependency used for publishing events.</param>
    /// <param name="modHelper">Dependency for events, input, and content.</param>
    protected ConfigManager(IDataHelper dataHelper, IEventPublisher eventPublisher, IModHelper modHelper)
    {
        this.dataHelper = dataHelper;
        this.eventPublisher = eventPublisher;
        this.modHelper = modHelper;
        this.Config = this.GetNew();
    }

    /// <summary>Gets the backing config.</summary>
    protected TConfig Config { get; private set; }

    /// <summary>Perform initialization routine.</summary>
    public void Init()
    {
        if (this.initialized)
        {
            return;
        }

        this.initialized = true;
        this.eventPublisher.Publish(new ConfigChangedEventArgs<TConfig>(this.Config));
    }

    /// <summary>Returns a new instance of IModConfig.</summary>
    /// <returns>The new instance of IModConfig.</returns>
    public virtual TConfig GetDefault() => new();

    /// <summary>Returns a new instance of IModConfig by reading the DefaultConfig from the mod helper.</summary>
    /// <returns>The new instance of IModConfig.</returns>
    public virtual TConfig GetNew()
    {
        TConfig? config;

        // Try to load config from mod folder
        try
        {
            return this.modHelper.ReadConfig<TConfig>();
        }
        catch
        {
            // ignored
        }

        // Try to restore from global data
        try
        {
            return this.dataHelper.ReadGlobalData<TConfig>("config") ?? throw new InvalidOperationException();
        }
        catch
        {
            // ignored
        }

        return this.GetDefault();
    }

    /// <summary>Resets the configuration by reassigning to <see cref="TConfig" />.</summary>
    public void Reset()
    {
        this.Config = this.GetNew();
        this.eventPublisher.Publish(new ConfigChangedEventArgs<TConfig>(this.Config));
    }

    /// <summary>Saves the provided config.</summary>
    /// <param name="config">The config object to be saved.</param>
    public void Save(TConfig config)
    {
        this.modHelper.WriteConfig(config);
        this.dataHelper.WriteGlobalData("config", config);
        this.Config = config;
        this.eventPublisher.Publish(new ConfigChangedEventArgs<TConfig>(this.Config));
    }
}