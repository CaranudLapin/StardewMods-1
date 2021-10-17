﻿namespace XSPlus.Services
{
    using System;
    using System.Collections.Generic;
    using Common.Integrations.GenericModConfigMenu;
    using Common.Services;
    using Models;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;

    /// <summary>
    ///     Service to handle read/write to <see cref="Models.ModConfig" />.
    /// </summary>
    internal class ModConfigService : BaseService
    {
        private readonly Action<string> _activateFeature;
        private readonly Action<string> _deactivateFeature;
        private readonly IManifest _manifest;
        private readonly GenericModConfigMenuIntegration _modConfigMenu;
        private readonly ITranslationHelper _translation;
        private readonly Action<ModConfig> _writeConfig;

        private ModConfigService(ServiceManager serviceManager)
            : base("ModConfig")
        {
            // Init
            this.ModConfig = serviceManager.Helper.ReadConfig<ModConfig>();
            this._activateFeature = serviceManager.ActivateFeature;
            this._deactivateFeature = serviceManager.DeactivateFeature;
            this._manifest = serviceManager.ModManifest;
            this._modConfigMenu = new(serviceManager.Helper.ModRegistry);
            this._translation = serviceManager.Helper.Translation;
            this._writeConfig = serviceManager.Helper.WriteConfig;

            // Events
            serviceManager.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }

        /// <summary>
        ///     Gets config containing default values and config options for features.
        /// </summary>
        public ModConfig ModConfig { get; private set; }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            if (!this._modConfigMenu.IsLoaded)
            {
                return;
            }

            // Register mod configuration
            this._modConfigMenu.API.Register(this._manifest, this.Reset, this.Save);

            // Config options
            this._modConfigMenu.API.AddSectionTitle(this._manifest, () => this._translation.Get("section.general.name"));
            this._modConfigMenu.API.AddKeybindList(this._manifest,
                name: () => this._translation.Get("config.open-crafting.name"),
                tooltip: () => this._translation.Get("config.open-crafting.tooltip"),
                getValue: () => this.ModConfig.OpenCrafting,
                setValue: value => this.ModConfig.OpenCrafting = value);

            this._modConfigMenu.API.AddKeybindList(
                this._manifest,
                name: () => this._translation.Get("config.stash-items.name"),
                tooltip: () => this._translation.Get("config.stash-items.tooltip"),
                getValue: () => this.ModConfig.StashItems,
                setValue: value => this.ModConfig.StashItems = value);

            this._modConfigMenu.API.AddKeybindList(
                this._manifest,
                name: () => this._translation.Get("config.scroll-up.name"),
                tooltip: () => this._translation.Get("config.scroll-up.tooltip"),
                getValue: () => this.ModConfig.ScrollUp,
                setValue: value => this.ModConfig.ScrollUp = value);

            this._modConfigMenu.API.AddKeybindList(
                this._manifest,
                name: () => this._translation.Get("config.scroll-down.name"),
                tooltip: () => this._translation.Get("config.scroll-down.tooltip"),
                getValue: () => this.ModConfig.ScrollDown,
                setValue: value => this.ModConfig.ScrollDown = value);

            this._modConfigMenu.API.AddKeybindList(
                this._manifest,
                name: () => this._translation.Get("config.previous-tab.name"),
                tooltip: () => this._translation.Get("config.previous-tab.tooltip"),
                getValue: () => this.ModConfig.PreviousTab,
                setValue: value => this.ModConfig.PreviousTab = value);

            this._modConfigMenu.API.AddKeybindList(this._manifest,
                name: () => this._translation.Get("config.next-tab.name"),
                tooltip: () => this._translation.Get("config.previous-tab.tooltip"),
                getValue: () => this.ModConfig.NextTab,
                setValue: value => this.ModConfig.NextTab = value);

            this._modConfigMenu.API.AddNumberOption(
                this._manifest,
                name: () => this._translation.Get("config.capacity.name"),
                tooltip: () => this._translation.Get("config.capacity.tooltip"),
                getValue: () => this.ModConfig.Capacity,
                setValue: this.SetCapacity);

            this._modConfigMenu.API.AddNumberOption(
                this._manifest,
                name: () => this._translation.Get("config.menu-rows.name"),
                tooltip: () => this._translation.Get("config.menu-rows.tooltip"),
                getValue: () => this.ModConfig.MenuRows,
                setValue: this.SetMenuRows,
                min: 3,
                max: 6,
                interval: 1);

            var rangeChoices = new[]
            {
                "Inventory", "Location", "World", "Default", "Disabled",
            };

            var rangeValues = new String[]
            {
                this._translation.Get("choice.inventory.name"),
                this._translation.Get("choice.location.name"),
                this._translation.Get("choice.world.name"),
                this._translation.Get("choice.default.name"),
                this._translation.Get("choice.disabled.name"),
            };

            this._modConfigMenu.API.AddTextOption(
                this._manifest,
                name: () => this._translation.Get("config.crafting-range.name"),
                tooltip: () => this._translation.Get("config.crafting-range.tooltip"),
                getValue: () => this.ModConfig.CraftingRange,
                setValue: this.SetCraftingRange,
                allowedValues: rangeChoices,
                labels: rangeValues);

            this._modConfigMenu.API.AddTextOption(
                this._manifest,
                name: () => this._translation.Get("config.stashing-range.name"),
                tooltip: () => this._translation.Get("config.stashing-range.tooltip"),
                getValue: () => this.ModConfig.StashingRange,
                setValue: this.SetStashingRange,
                allowedValues: rangeChoices,
                labels: rangeValues);

            var configChoices = new[]
            {
                "Default", "Enable", "Disable",
            };

            var configValues = new String[]
            {
                this._translation.Get("choice.default.name"),
                this._translation.Get("choice.enabled.name"),
                this._translation.Get("choice.disabled.name"),
            };

            this._modConfigMenu.API.AddSectionTitle(
                this._manifest, 
                () => this._translation.Get("section.global-overrides.name"), 
                () => this._translation.Get("section.global-overrides.tooltip"));

            this._modConfigMenu.API.AddTextOption(
                this._manifest,
                name: () => this._translation.Get("config.access-carried.name"),
                tooltip: () => this._translation.Get("config.access-carried.tooltip"),
                getValue: this.GetConfig("AccessCarried"),
                setValue: this.SetConfig("AccessCarried"),
                allowedValues: configChoices,
                labels: configValues);

            this._modConfigMenu.API.AddTextOption(
                this._manifest,
                name: () => this._translation.Get("config.carry-chest.name"),
                tooltip: () => this._translation.Get("config.carry-chest.tooltip"),
                getValue: this.GetConfig("CarryChest"),
                setValue: this.SetConfig("CarryChest"),
                allowedValues: configChoices,
                labels: configValues);

            this._modConfigMenu.API.AddTextOption(
                this._manifest,
                name: () => this._translation.Get("config.categorize-chest.name"),
                tooltip: () => this._translation.Get("config.categorize-chest.tooltip"),
                getValue: this.GetConfig("CategorizeChest"),
                setValue: this.SetConfig("CategorizeChest"),
                allowedValues: configChoices,
                labels: configValues);

            this._modConfigMenu.API.AddTextOption(
                this._manifest,
                name: () => this._translation.Get("config.color-picker.name"),
                tooltip: () => this._translation.Get("config.color-picker.tooltip"),
                getValue: this.GetConfig("ColorPicker"),
                setValue: this.SetConfig("ColorPicker"),
                allowedValues: configChoices,
                labels: configValues);

            this._modConfigMenu.API.AddTextOption(
                this._manifest,
                name: () => this._translation.Get("config.inventory-tabs.name"),
                tooltip: () => this._translation.Get("config.inventory-tabs.tooltip"),
                getValue: this.GetConfig("InventoryTabs"),
                setValue: this.SetConfig("InventoryTabs"),
                allowedValues: configChoices,
                labels: configValues);

            this._modConfigMenu.API.AddTextOption(
                this._manifest,
                name: () => this._translation.Get("config.search-items.name"),
                tooltip: () => this._translation.Get("config.search-items.tooltip"),
                getValue: this.GetConfig("SearchItems"),
                setValue: this.SetConfig("SearchItems"),
                allowedValues: configChoices,
                labels: configValues);

            this._modConfigMenu.API.AddTextOption(
                this._manifest,
                name: () => this._translation.Get("config.vacuum-items.name"),
                tooltip: () => this._translation.Get("config.vacuum-items.tooltip"),
                getValue: this.GetConfig("VacuumItems"),
                setValue: this.SetConfig("VacuumItems"),
                allowedValues: configChoices,
                labels: configValues);
        }

        private void Reset()
        {
            this.ModConfig = new();
        }

        private void Save()
        {
            this._writeConfig(this.ModConfig);
        }

        private Func<string> GetConfig(string featureName)
        {
            return () => this.ModConfig.Global.TryGetValue(featureName, out var global)
                ? global ? "Enable" : "Disable"
                : "Default";
        }

        private Action<string> SetConfig(string featureName)
        {
            return value =>
            {
                switch (value)
                {
                    case "Enable":
                        this.ModConfig.Global[featureName] = true;
                        this._activateFeature(featureName);
                        break;
                    case "Disable":
                        this.ModConfig.Global[featureName] = false;
                        this._deactivateFeature(featureName);
                        break;
                    default:
                        this.ModConfig.Global.Remove(featureName);
                        this._activateFeature(featureName);
                        break;
                }
            };
        }

        private void SetCapacity(int value)
        {
            this.ModConfig.Capacity = value;
            if (value == 0)
            {
                this.ModConfig.Global.Remove("Capacity");
            }
            else
            {
                this.ModConfig.Global["Capacity"] = true;
            }
        }

        private void SetMenuRows(int value)
        {
            this.ModConfig.MenuRows = value;
            if (value <= 3)
            {
                this.ModConfig.Global.Remove("ExpandedMenu");
            }
            else
            {
                this.ModConfig.Global["ExpandedMenu"] = true;
            }
        }

        private void SetCraftingRange(string value)
        {
            switch (value)
            {
                case "Default":
                    this.ModConfig.CraftingRange = "Location";
                    this.ModConfig.Global.Remove("CraftFromChest");
                    break;
                case "Disabled":
                    this.ModConfig.Global["CraftFromChest"] = false;
                    break;
                default:
                    this.ModConfig.CraftingRange = value;
                    this.ModConfig.Global["CraftFromChest"] = true;
                    break;
            }
        }

        private void SetStashingRange(string value)
        {
            switch (value)
            {
                case "Default":
                    this.ModConfig.StashingRange = "Location";
                    this.ModConfig.Global.Remove("StashToChest");
                    break;
                case "Disabled":
                    this.ModConfig.Global["StashToChest"] = false;
                    break;
                default:
                    this.ModConfig.StashingRange = value;
                    this.ModConfig.Global["StashToChest"] = true;
                    break;
            }
        }
    }
}