namespace StardewMods.BetterChests.Framework.Models.Containers;

using Microsoft.Xna.Framework;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewValley.Buildings;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Mods;
using StardewValley.Network;
using StardewValley.Objects;

/// <inheritdoc />
internal sealed class BuildingContainer : BaseContainer<Building>
{
    private readonly Chest? chest;

    /// <summary>Initializes a new instance of the <see cref="BuildingContainer" /> class.</summary>
    /// <param name="baseOptions">The type of storage object.</param>
    /// <param name="building">The building to which the storage is connected.</param>
    /// <param name="chest">The chest storage of the container.</param>
    public BuildingContainer(IStorageOptions baseOptions, Building building, Chest chest)
        : base(baseOptions)
    {
        this.Source = new WeakReference<Building>(building);
        this.chest = chest;
    }

    /// <summary>Initializes a new instance of the <see cref="BuildingContainer" /> class.</summary>
    /// <param name="baseOptions">The type of storage object.</param>
    /// <param name="shippingBin">The building to which the storage is connected.</param>
    public BuildingContainer(IStorageOptions baseOptions, ShippingBin shippingBin)
        : base(baseOptions) =>
        this.Source = new WeakReference<Building>(shippingBin);

    /// <summary>Gets the source building of the container.</summary>
    /// <exception cref="ObjectDisposedException">Thrown when the Building is disposed.</exception>
    public Building Building =>
        this.Source.TryGetTarget(out var target)
            ? target
            : throw new ObjectDisposedException(nameof(BuildingContainer));

    /// <inheritdoc />
    public override int Capacity => this.chest?.GetActualCapacity() ?? int.MaxValue;

    /// <inheritdoc />
    public override IInventory Items => this.chest?.GetItemsForPlayer() ?? Game1.getFarm().getShippingBin(Game1.player);

    /// <inheritdoc />
    public override GameLocation Location => this.Building.GetParentLocation();

    /// <inheritdoc />
    public override Vector2 TileLocation =>
        new(
            this.Building.tileX.Value + (this.Building.tilesWide.Value / 2f),
            this.Building.tileY.Value + (this.Building.tilesHigh.Value / 2f));

    /// <inheritdoc />
    public override ModDataDictionary ModData => this.Building.modData;

    /// <inheritdoc />
    public override NetMutex? Mutex => this.chest?.GetMutex();

    /// <inheritdoc />
    public override bool IsAlive => this.Source.TryGetTarget(out _);

    /// <inheritdoc />
    public override WeakReference<Building> Source { get; }

    /// <inheritdoc />
    public override void ShowMenu(bool playSound = false)
    {
        var itemGrabMenu = this.Building switch
        {
            ShippingBin when this.Options.ResizeChest is ChestMenuOption.Default or ChestMenuOption.Disabled =>
                new ItemGrabMenu(
                    null,
                    true,
                    false,
                    this.HighlightItems,
                    this.GrabItemFromInventory,
                    null,
                    null,
                    true,
                    true,
                    false,
                    true,
                    false,
                    0,
                    null,
                    -1,
                    this),
            ShippingBin => new ItemGrabMenu(
                this.Items,
                false,
                true,
                this.HighlightItems,
                this.GrabItemFromInventory,
                null,
                this.GrabItemFromChest,
                false,
                true,
                true,
                true,
                true,
                0,
                null,
                -1,
                this),
            JunimoHut => new ItemGrabMenu(
                this.Items,
                false,
                true,
                this.HighlightItems,
                this.GrabItemFromInventory,
                null,
                this.GrabItemFromChest,
                false,
                true,
                true,
                true,
                true,
                1,
                null,
                1,
                this.Building),
            _ => null,
        };

        if (itemGrabMenu is null)
        {
            base.ShowMenu(playSound);
            return;
        }

        if (this.Building is ShippingBin
            && this.Options.ResizeChest is ChestMenuOption.Default or ChestMenuOption.Disabled)
        {
            itemGrabMenu.initializeUpperRightCloseButton();
            itemGrabMenu.setBackgroundTransparency(b: false);
            itemGrabMenu.setDestroyItemOnClick(b: true);
            itemGrabMenu.initializeShippingBin();
        }

        Game1.activeClickableMenu = itemGrabMenu;
    }

    /// <inheritdoc />
    public override bool TryAdd(Item item, out Item? remaining)
    {
        var stack = item.Stack;
        if (this.chest is not null)
        {
            remaining = this.chest.addItem(item);
            return remaining is null || remaining.Stack != stack;
        }

        remaining = null;
        foreach (var slot in this.Items)
        {
            if (slot?.canStackWith(item) != true)
            {
                continue;
            }

            item.Stack = slot.addToStack(item);
            if (item.Stack > 0)
            {
                continue;
            }

            Game1.getFarm().lastItemShipped = slot;
            return true;
        }

        this.Items.Add(item);
        Game1.getFarm().lastItemShipped = item;
        return true;
    }

    /// <inheritdoc />
    public override bool TryRemove(Item item)
    {
        if (!this.Items.Contains(item))
        {
            return false;
        }

        this.Items.Remove(item);
        this.Items.RemoveEmptySlots();
        return true;
    }

    /// <inheritdoc />
    public override void GrabItemFromChest(Item? item, Farmer who)
    {
        if (item is null || !who.couldInventoryAcceptThisItem(item))
        {
            return;
        }

        if (this.Building is ShippingBin && item == Game1.getFarm().lastItemShipped)
        {
            Game1.getFarm().lastItemShipped = this.Items.LastOrDefault();
        }

        this.Items.RemoveEmptySlots();
        this.ShowMenu();
    }

    /// <inheritdoc />
    public override bool HighlightItems(Item? item) =>
        this.Building is ShippingBin ? Utility.highlightShippableObjects(item) : base.HighlightItems(item);
}