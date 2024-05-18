namespace StardewMods.ToolbarIcons.Framework.Interfaces;

/// <summary>Represents an integration which is directly supported by this mod.</summary>
internal interface ICustomIntegration
{
    /// <summary>Gets the text used when hovering over the toolbar icon.</summary>
    string HoverText { get; }

    /// <summary>Gets the unique identifier for the icon.</summary>
    string Icon { get; }
}