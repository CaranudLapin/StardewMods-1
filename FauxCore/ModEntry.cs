namespace StardewMods.FauxCore;

using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.FauxCore.Framework.Interfaces;
using StardewMods.FauxCore.Framework.Services;

/// <inheritdoc />
internal sealed class ModEntry : Mod
{
    /// <inheritdoc />
    public override object GetApi(IModInfo mod) => this.CreateApi(mod);

    /// <inheritdoc />
    protected override void Init(Container container)
    {
        I18n.Init(this.Helper.Translation);
        container.RegisterSingleton<IApiFactory, ApiFactory>();
        container.RegisterSingleton<IAssetHandlerExtension, AssetHandler>();
        container.RegisterSingleton<IThemeHelper, AssetHandler>();
        container.RegisterSingleton<CacheManager>();
        container.RegisterSingleton<IModConfig, ConfigManager>();
        container.RegisterSingleton<ConfigManager, ConfigManager>();
        container.RegisterSingleton<ContentPatcherIntegration>();
        container.RegisterSingleton<IEventManager, EventManager>();
        container.RegisterSingleton<IExpressionHandler, ExpressionHandler>();
        container.RegisterSingleton<GenericModConfigMenuIntegration>();
        container.RegisterSingleton<ISimpleLogging, SimpleLogging>();
    }
}