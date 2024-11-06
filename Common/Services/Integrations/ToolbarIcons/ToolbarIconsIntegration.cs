﻿#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.Services.Integrations.ToolbarIcons;
#else
namespace StardewMods.Common.Services.Integrations.ToolbarIcons;
#endif

/// <inheritdoc />
internal sealed class ToolbarIconsIntegration : ModIntegration<IToolbarIconsApi>
{
    /// <summary>Initializes a new instance of the <see cref="ToolbarIconsIntegration" /> class.</summary>
    /// <param name="modRegistry">Dependency used for fetching metadata about loaded mods.</param>
    public ToolbarIconsIntegration(IModRegistry modRegistry)
        : base(modRegistry)
    {
        // Nothing
    }

    /// <inheritdoc />
    public override string UniqueId => "furyx639.ToolbarIcons";

    /// <inheritdoc />
    public override ISemanticVersion Version { get; } = new SemanticVersion(2, 8, 0);
}