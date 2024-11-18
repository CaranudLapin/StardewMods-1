#if IS_FAUXCORE

namespace StardewMods.FauxCore.Common.Services.Integrations.ExpandedStorage;
#else

namespace StardewMods.Common.Services.Integrations.ExpandedStorage;
#endif

/// <inheritdoc />
internal sealed class ExpandedStorageIntegration : ModIntegration<IExpandedStorageApi>
{
    /// <summary>Initializes a new instance of the <see cref="ExpandedStorageIntegration" /> class.</summary>
    /// <param name="modRegistry">Dependency used for fetching metadata about loaded mods.</param>
    public ExpandedStorageIntegration(IModRegistry modRegistry)
        : base(modRegistry)
    {
        // Nothing
    }

    /// <inheritdoc />
    public override string UniqueId => "furyx639.ExpandedStorage";

    /// <inheritdoc />
    public override ISemanticVersion Version { get; } = new SemanticVersion(3, 1, 0);
}