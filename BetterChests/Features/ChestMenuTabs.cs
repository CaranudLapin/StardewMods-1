﻿namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using Common.Helpers;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Interfaces.Config;
using StardewMods.BetterChests.Models;
using StardewMods.BetterChests.Services;
using StardewMods.FuryCore.Attributes;
using StardewMods.FuryCore.Helpers;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Models.CustomEvents;
using StardewMods.FuryCore.UI;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc />
internal class ChestMenuTabs : Feature
{
    private readonly Lazy<AssetHandler> _assetHandler;
    private readonly PerScreen<object> _context = new();
    private readonly PerScreen<ItemMatcher> _itemMatcher = new(() => new(true));
    private readonly PerScreen<ItemGrabMenu> _menu = new();
    private readonly Lazy<IMenuItems> _menuItems;
    private readonly PerScreen<int> _tabIndex = new(() => -1);
    private readonly PerScreen<IList<TabComponent>> _tabs = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="ChestMenuTabs" /> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public ChestMenuTabs(IConfigModel config, IModHelper helper, IModServices services)
        : base(config, helper, services)
    {
        this._assetHandler = services.Lazy<AssetHandler>();
        this._menuItems = services.Lazy<IMenuItems>();
    }

    private AssetHandler Assets
    {
        get => this._assetHandler.Value;
    }

    private object Context
    {
        get => this._context.Value;
        set => this._context.Value = value;
    }

    private int Index
    {
        get => this._tabIndex.Value;
        set => this._tabIndex.Value = value;
    }

    private ItemMatcher ItemMatcher
    {
        get => this._itemMatcher.Value;
    }

    private ItemGrabMenu Menu
    {
        get => this._menu.Value;
        set => this._menu.Value = value;
    }

    private IMenuItems MenuItems
    {
        get => this._menuItems.Value;
    }

    private IList<TabComponent> Tabs
    {
        get => this._tabs.Value ??= (
                from tab in this.Assets.TabData
                select new TabComponent(
                    new(
                        new(0, 0, 16 * Game1.pixelZoom, 16 * Game1.pixelZoom),
                        this.Helper.Content.Load<Texture2D>(tab.Value[1], ContentSource.GameContent),
                        new(16 * int.Parse(tab.Value[2]), 0, 16, 16),
                        Game1.pixelZoom)
                    {
                        hoverText = tab.Value[0],
                        name = tab.Key,
                    },
                    tab.Value[3].Split(' ')))
            .ToList();
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.CustomEvents.MenuComponentsLoading += this.OnMenuComponentsLoading;
        this.CustomEvents.MenuComponentPressed += this.OnMenuComponentPressed;
        this.CustomEvents.MenuItemsChanged += this.OnMenuItemsChanged;
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
        this.Helper.Events.Input.MouseWheelScrolled += this.OnMouseWheelScrolled;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.CustomEvents.MenuComponentsLoading -= this.OnMenuComponentsLoading;
        this.CustomEvents.MenuComponentPressed -= this.OnMenuComponentPressed;
        this.CustomEvents.MenuItemsChanged -= this.OnMenuItemsChanged;
        this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
        this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
        this.Helper.Events.Input.MouseWheelScrolled -= this.OnMouseWheelScrolled;
    }

    [EventPriority(EventPriority.High + 10)]
    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if (this.Menu is not ItemSelectionMenu itemSelectionMenu || e.Button != SButton.MouseRight)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        var tab = this.Tabs.SingleOrDefault(tab => tab.Component.containsPoint(x, y));
        if (tab is null || !itemSelectionMenu.AddTagMenu(tab.Tags, x, y))
        {
            return;
        }

        this.Helper.Input.Suppress(e.Button);
    }

    private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
    {
        if (this.Menu is null)
        {
            return;
        }

        if (this.Config.ControlScheme.NextTab.JustPressed())
        {
            this.SetTab(this.Index == this.Tabs.Count - 1 ? -1 : this.Index + 1);
            this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.NextTab);
            return;
        }

        if (this.Config.ControlScheme.PreviousTab.JustPressed())
        {
            this.SetTab(this.Index == -1 ? this.Tabs.Count - 1 : this.Index - 1);
            this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.PreviousTab);
        }
    }

    [SortedEventPriority(EventPriority.Low)]
    private void OnMenuComponentPressed(object sender, ClickableComponentPressedEventArgs e)
    {
        if (e.Component is not TabComponent tab || e.IsSuppressed())
        {
            return;
        }

        var index = this.Tabs.IndexOf(tab);
        if (index == -1)
        {
            return;
        }

        if (e.Button is SButton.Left || e.Button.IsActionButton())
        {
            this.SetTab(this.Index == index ? -1 : index);
        }

        e.SuppressInput();
    }

    private void OnMenuComponentsLoading(object sender, MenuComponentsLoadingEventArgs e)
    {
        IStorageData storageData;
        var resetTab = false;
        switch (e.Menu)
        {
            case ItemSelectionMenu itemSelectionMenu when this.Config.DefaultChest.ChestMenuTabs == FeatureOption.Enabled:
                this.Menu = itemSelectionMenu;
                storageData = this.Config.DefaultChest;
                break;

            case ItemGrabMenu { context: { } context } itemGrabMenu when this.ManagedObjects.TryGetManagedStorage(context, out var managedStorage) && managedStorage.ChestMenuTabs == FeatureOption.Enabled:
                this.Menu = itemGrabMenu;
                storageData = managedStorage;
                if (!ReferenceEquals(context, this.Context))
                {
                    this.Context = context;
                    resetTab = true;
                }

                break;

            default:
                this.Menu = null;
                return;
        }

        var tabs = (
            from tabSet in storageData.ChestMenuTabSet.Select((name, index) => (name, index))
            join tabData in this.Tabs on tabSet.name equals tabData.Name
            orderby tabSet.index
            select tabData).ToList();
        foreach (var tab in tabs.Any() ? tabs : this.Tabs)
        {
            e.AddComponent(tab);
        }

        if (resetTab)
        {
            this.SetTab(-1);
        }
    }

    private void OnMenuItemsChanged(object sender, IMenuItemsChangedEventArgs e)
    {
        switch (e.Menu)
        {
            case ItemSelectionMenu when this.Config.DefaultChest.ChestMenuTabs == FeatureOption.Enabled:
            case not null when e.Context is not null && this.ManagedObjects.TryGetManagedStorage(e.Context, out var managedStorage) && managedStorage.ChestMenuTabs == FeatureOption.Enabled:
                e.AddFilter(this.ItemMatcher);
                break;
        }
    }

    private void OnMouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
    {
        if (this.MenuItems.Menu is null)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (!this.Tabs.Any(tab => tab.Component.containsPoint(x, y)))
        {
            return;
        }

        switch (e.Delta)
        {
            case > 0:
                this.SetTab(this.Index == -1 ? this.Tabs.Count - 1 : this.Index - 1);
                break;
            case < 0:
                this.SetTab(this.Index == this.Tabs.Count - 1 ? -1 : this.Index + 1);
                break;
            default:
                return;
        }
    }

    private void SetTab(int index)
    {
        if (this.Index != -1)
        {
            this.Tabs[this.Index].Selected = false;
        }

        this.Index = index;
        if (this.Index != -1)
        {
            Log.Trace($"Switching to Tab {this.Tabs[this.Index].Name}.");
            this.Tabs[this.Index].Selected = true;
        }

        this.ItemMatcher.Clear();
        if (index != -1)
        {
            foreach (var tag in this.Tabs[this.Index].Tags)
            {
                this.ItemMatcher.Add(tag);
            }
        }
    }
}