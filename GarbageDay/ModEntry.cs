namespace StardewMods.GarbageDay;

using SimpleInjector;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.ToolbarIcons;
using StardewMods.GarbageDay.Framework.Interfaces;
using StardewMods.GarbageDay.Framework.Models;
using StardewMods.GarbageDay.Framework.Services;

/// <inheritdoc />
public sealed class ModEntry : Mod
{
    private Container container = null!;

    /// <inheritdoc />
    protected override void Init()
    {
        // Init
        I18n.Init(this.Helper.Translation);
        this.container = new Container();

        // Configuration
        this.container.RegisterInstance(this.Helper);
        this.container.RegisterInstance(this.ModManifest);
        this.container.RegisterInstance(this.Monitor);
        this.container.RegisterInstance(this.Helper.Data);
        this.container.RegisterInstance(this.Helper.Events);
        this.container.RegisterInstance(this.Helper.GameContent);
        this.container.RegisterInstance(this.Helper.Input);
        this.container.RegisterInstance(this.Helper.ModContent);
        this.container.RegisterInstance(this.Helper.ModRegistry);
        this.container.RegisterInstance(this.Helper.Reflection);
        this.container.RegisterInstance(this.Helper.Translation);

        this.container.RegisterSingleton<AssetHandler>();
        this.container.RegisterSingleton<IModConfig, ConfigManager>();
        this.container.RegisterSingleton<ContentPatcherIntegration>();
        this.container.RegisterSingleton<IEventManager, EventManager>();
        this.container.RegisterSingleton<FauxCoreIntegration>();
        this.container.RegisterSingleton<GarbageCanManager>();
        this.container.RegisterSingleton<ISimpleLogging, FauxCoreIntegration>();
        this.container.RegisterSingleton<Log>();
        this.container.RegisterSingleton<ToolbarIconsIntegration>();

        this.container.RegisterInstance(new Dictionary<string, FoundGarbageCan>());

        // Verify
        this.container.Verify();
    }
}