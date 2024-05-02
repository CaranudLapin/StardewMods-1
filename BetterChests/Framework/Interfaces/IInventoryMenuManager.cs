namespace StardewMods.BetterChests.Framework.Interfaces;

using StardewMods.Common.Services.Integrations.BetterChests;
using StardewValley.Menus;

/// <summary>Manages the inventory menu by adding, removing, and filtering item filters.</summary>
internal interface IInventoryMenuManager
{
    /// <summary>Gets the instance of the inventory menu that is being managed.</summary>
    public InventoryMenu? Menu { get; }

    /// <summary>Gets the container associated with the inventory menu.</summary>
    public IStorageContainer? Container { get; }

    /// <summary>Gets the capacity of the inventory menu.</summary>
    public int Capacity { get; }

    /// <summary>Gets the number of columns of the inventory menu.</summary>
    public int Columns { get; }

    /// <summary>Gets the number of rows of the inventory menu.</summary>
    public int Rows { get; }
}