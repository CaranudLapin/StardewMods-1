namespace StardewMods.BetterChests.Framework.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Models.StorageOptions;
using StardewMods.Common.Helpers;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Models.Events;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Locations;

/// <summary>Responsible for handling assets provided by this mod.</summary>
internal sealed class AssetHandler : BaseService
{
    private readonly IGameContentHelper gameContentHelper;
    private readonly string hslTexturePath;
    private readonly IIconRegistry iconRegistry;
    private readonly IModConfig modConfig;
    private HslColor[]? hslColors;
    private Texture2D? hslTexture;
    private Color[]? hslTextureData;

    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="log">Dependency used for logging information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for managing config data.</param>
    /// <param name="modContentHelper">Dependency used for accessing mod content.</param>
    /// <param name="themeHelper">Dependency used for swapping palettes.</param>
    public AssetHandler(
        IEventManager eventManager,
        IGameContentHelper gameContentHelper,
        IIconRegistry iconRegistry,
        ILog log,
        IManifest manifest,
        IModConfig modConfig,
        IModContentHelper modContentHelper,
        IThemeHelper themeHelper)
        : base(log, manifest)
    {
        // Init
        this.gameContentHelper = gameContentHelper;
        this.iconRegistry = iconRegistry;
        this.modConfig = modConfig;
        this.hslTexturePath = this.ModId + "/HueBar";

        var data = modContentHelper.Load<IRawTextureData>("assets/icons.png");
        themeHelper.AddAsset(this.ModId + "/UI", data);

        // Events
        eventManager.Subscribe<AssetRequestedEventArgs>(this.OnAssetRequested);
        eventManager.Subscribe<ConfigChangedEventArgs<DefaultConfig>>(this.OnConfigChanged);
        eventManager.Subscribe<GameLaunchedEventArgs>(this.OnGameLaunched);
    }

    /// <summary>Gets the hsl colors data.</summary>
    public HslColor[] HslColors
    {
        get
        {
            if (this.hslTextureData is not null)
            {
                return this.hslColors ??= this.hslTextureData.Select(HslColor.FromColor).Distinct().ToArray();
            }

            this.hslTextureData = new Color[this.HslTexture.Width * this.HslTexture.Height];
            this.HslTexture.GetData(this.hslTextureData);
            return this.hslColors ??= this.hslTextureData.Select(HslColor.FromColor).Distinct().ToArray();
        }
    }

    /// <summary>Gets the hsl texture.</summary>
    public Texture2D HslTexture => this.hslTexture ??= this.gameContentHelper.Load<Texture2D>(this.hslTexturePath);

    private void OnAssetRequested(AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(this.hslTexturePath))
        {
            e.LoadFromModFile<Texture2D>("assets/hue.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo("Data/BigCraftables")
            && this.modConfig.StorageOptions.TryGetValue("BigCraftables", out var storageTypes))
        {
            e.Edit(
                asset =>
                {
                    var data = asset.AsDictionary<string, BigCraftableData>().Data;
                    foreach (var (storageId, storageOptions) in storageTypes)
                    {
                        if (!data.TryGetValue(storageId, out var bigCraftableData))
                        {
                            continue;
                        }

                        bigCraftableData.CustomFields ??= new Dictionary<string, string>();
                        var typeOptions = new CustomFieldsStorageOptions(() => bigCraftableData.CustomFields);
                        storageOptions.CopyTo(typeOptions);
                    }
                });

            return;
        }

        if (e.Name.IsEquivalentTo("Data/Buildings")
            && this.modConfig.StorageOptions.TryGetValue("Buildings", out storageTypes))
        {
            e.Edit(
                asset =>
                {
                    var data = asset.AsDictionary<string, BuildingData>().Data;
                    foreach (var (storageId, storageOptions) in storageTypes)
                    {
                        if (!data.TryGetValue(storageId, out var buildingData))
                        {
                            continue;
                        }

                        buildingData.CustomFields ??= new Dictionary<string, string>();
                        var typeOptions = new CustomFieldsStorageOptions(() => buildingData.CustomFields);
                        storageOptions.CopyTo(typeOptions);
                    }
                });

            return;
        }

        if (e.Name.IsEquivalentTo("Data/Locations")
            && this.modConfig.StorageOptions.TryGetValue("Locations", out storageTypes))
        {
            e.Edit(
                asset =>
                {
                    var data = asset.AsDictionary<string, LocationData>().Data;
                    foreach (var (storageId, storageOptions) in storageTypes)
                    {
                        if (!data.TryGetValue(storageId, out var locationData))
                        {
                            continue;
                        }

                        locationData.CustomFields ??= new Dictionary<string, string>();
                        var typeOptions = new CustomFieldsStorageOptions(() => locationData.CustomFields);
                        storageOptions.CopyTo(typeOptions);
                    }
                });
        }
    }

    private void OnConfigChanged(ConfigChangedEventArgs<DefaultConfig> e)
    {
        foreach (var dataType in e.Config.StorageOptions.Keys)
        {
            this.gameContentHelper.InvalidateCache($"Data/{dataType}");
        }
    }

    private void OnGameLaunched(GameLaunchedEventArgs e)
    {
        var icons = new[]
        {
            "Clothing",
            "Cooking",
            "Crops",
            "Equipment",
            "Fishing",
            "Materials",
            "Miscellaneous",
            "Seeds",
            "Config",
            "Stash",
            "Craft",
            "Search",
            "Copy",
            "Save",
            "Paste",
            "TransferUp",
            "TransferDown",
            "HSL",
            "Debug",
            "NoStack",
        };

        for (var index = 0; index < icons.Length; index++)
        {
            this.iconRegistry.AddIcon(
                icons[index],
                $"{this.ModId}/UI",
                new Rectangle(16 * (index % 5), 16 * (int)(index / 5f), 16, 16));
        }
    }
}