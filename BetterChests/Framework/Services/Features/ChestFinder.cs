namespace StardewMods.BetterChests.Framework.Services.Features;

using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Containers;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.BetterChests.Framework.UI.Overlays;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.ToolbarIcons;

/// <summary>Search for which chests have the item you're looking for.</summary>
internal sealed class ChestFinder : BaseFeature<ChestFinder>
{
    private readonly AssetHandler assetHandler;
    private readonly ContainerFactory containerFactory;
    private readonly PerScreen<int> currentIndex = new();
    private readonly ExpressionHandler expressionHandler;
    private readonly IInputHelper inputHelper;
    private readonly MenuHandler menuHandler;
    private readonly PerScreen<List<Pointer>> pointers = new(() => []);
    private readonly ToolbarIconsIntegration toolbarIconsIntegration;

    /// <summary>Initializes a new instance of the <see cref="ChestFinder" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="menuHandler">Dependency used for managing the current menu.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="toolbarIconsIntegration">Dependency for Toolbar Icons integration.</param>
    public ChestFinder(
        AssetHandler assetHandler,
        ContainerFactory containerFactory,
        IEventManager eventManager,
        ExpressionHandler expressionHandler,
        IInputHelper inputHelper,
        ILog log,
        IManifest manifest,
        MenuHandler menuHandler,
        IModConfig modConfig,
        ToolbarIconsIntegration toolbarIconsIntegration)
        : base(eventManager, log, manifest, modConfig)
    {
        this.assetHandler = assetHandler;
        this.containerFactory = containerFactory;
        this.expressionHandler = expressionHandler;
        this.inputHelper = inputHelper;
        this.menuHandler = menuHandler;
        this.toolbarIconsIntegration = toolbarIconsIntegration;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.ChestFinder != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<RenderedHudEventArgs>(this.OnRenderedHud);
        this.Events.Subscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.Events.Subscribe<MenuChangedEventArgs>(this.OnMenuChanged);
        this.Events.Subscribe<SearchChangedEventArgs>(this.OnSearchChanged);
        this.Events.Subscribe<WarpedEventArgs>(this.OnWarped);

        // Integrations
        if (!this.toolbarIconsIntegration.IsLoaded
            || !this.assetHandler.Icons.TryGetValue(this.ModId + "/Search", out var icon))
        {
            return;
        }

        this.toolbarIconsIntegration.Api.AddToolbarIcon(this.Id, icon.Path, icon.Area, I18n.Button_FindChest_Name());

        this.toolbarIconsIntegration.Api.Subscribe(this.OnIconPressed);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<RenderedHudEventArgs>(this.OnRenderedHud);
        this.Events.Unsubscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.Events.Subscribe<MenuChangedEventArgs>(this.OnMenuChanged);
        this.Events.Subscribe<SearchChangedEventArgs>(this.OnSearchChanged);
        this.Events.Unsubscribe<WarpedEventArgs>(this.OnWarped);

        // Integrations
        if (!this.toolbarIconsIntegration.IsLoaded)
        {
            return;
        }

        this.toolbarIconsIntegration.Api.RemoveToolbarIcon(this.Id);
        this.toolbarIconsIntegration.Api.Unsubscribe(this.OnIconPressed);
    }

    private void OnButtonsChanged(ButtonsChangedEventArgs e)
    {
        // Activate Search Bar
        if (Context.IsPlayerFree
            && Game1.displayHUD
            && this.menuHandler.CurrentMenu is null
            && this.Config.Controls.ToggleSearch.JustPressed())
        {
            this.inputHelper.SuppressActiveKeybinds(this.Config.Controls.ToggleSearch);
            this.OpenSearchBar();
            return;
        }

        if (this.menuHandler.CurrentMenu is not SearchOverlay)
        {
            return;
        }

        // Open Found Chest
        if (this.pointers.Value.Any() && this.Config.Controls.OpenFoundChest.JustPressed())
        {
            this.inputHelper.SuppressActiveKeybinds(this.Config.Controls.OpenFoundChest);
            var container = this.pointers.Value.First().Container;
            container.Mutex?.RequestLock(
                () =>
                {
                    container.ShowMenu();
                });

            return;
        }

        // Clear Search
        if (this.Config.Controls.ClearSearch.JustPressed())
        {
            this.expressionHandler.SearchText = string.Empty;
            this.expressionHandler.SearchExpression = null;
            this.Events.Publish(new SearchChangedEventArgs(this.expressionHandler.SearchExpression));
        }
    }

    private void OnIconPressed(IIconPressedEventArgs e)
    {
        if (e.Id == this.Id)
        {
            this.OpenSearchBar();
        }
    }

    private void OnMenuChanged(MenuChangedEventArgs e) => this.ReinitializePointers();

    private void OnRenderedHud(RenderedHudEventArgs e)
    {
        if (this.menuHandler.CurrentMenu is not SearchOverlay && (!Game1.displayHUD || !Context.IsPlayerFree))
        {
            return;
        }

        foreach (var pointer in this.pointers.Value)
        {
            pointer.Draw(e.SpriteBatch);
        }
    }

    private void OnSearchChanged(SearchChangedEventArgs e) => this.ReinitializePointers();

    private void OnWarped(WarpedEventArgs e) => this.ReinitializePointers();

    private void OpenSearchBar() =>
        Game1.activeClickableMenu = new SearchOverlay(
            () => this.expressionHandler.SearchText,
            value =>
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.Log.Trace("{0}: Searching for {1}", this.Id, value);
                }

                if (this.expressionHandler.SearchText == value)
                {
                    return;
                }

                this.expressionHandler.SearchText = value;
                this.expressionHandler.SearchExpression =
                    this.expressionHandler.TryParseExpression(value, out var expression) ? expression : null;

                this.Events.Publish(new SearchChangedEventArgs(this.expressionHandler.SearchExpression));
            });

    private bool Predicate(IStorageContainer container) =>
        container is not FarmerContainer
        && container.ChestFinder is FeatureOption.Enabled
        && (this.expressionHandler.SearchExpression is null
            || this.expressionHandler.SearchExpression.Matches(container));

    private void ReinitializePointers()
    {
        this.pointers.Value.Clear();
        if (this.expressionHandler.SearchExpression is null)
        {
            return;
        }

        if (this.menuHandler.CurrentMenu is not SearchOverlay && (!Game1.displayHUD || !Context.IsPlayerFree))
        {
            return;
        }

        var containers = this.containerFactory.GetAll(Game1.player.currentLocation, this.Predicate);
        this.pointers.Value.AddRange(containers.Select(container => new Pointer(container)));
        this.Log.Info("{0}: Found {1} chests", this.Id, this.pointers.Value.Count);
        this.currentIndex.Value = 0;
    }
}