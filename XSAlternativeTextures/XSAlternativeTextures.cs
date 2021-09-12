﻿using System;
using System.Collections.Generic;
using System.Linq;
using AlternativeTextures.Framework.Models;
using Common.Integrations.XSLite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using IAlternativeTexturesAPI = AlternativeTextures.Framework.Interfaces.API.IApi;

namespace XSAlternativeTextures
{
    public class XSAlternativeTextures : Mod, IAssetEditor
    {
        private readonly IList<string> _storages = new List<string>();
        private IAlternativeTexturesAPI _alternativeTexturesAPI;
        private XSLiteIntegration _xsLite;
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            _xsLite = new XSLiteIntegration(helper.ModRegistry);
            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            _alternativeTexturesAPI = Helper.ModRegistry.GetApi<IAlternativeTexturesAPI>("PeacefulEnd.AlternativeTextures");
            var model = new AlternativeTextureModel
            {
                ItemName = "Chest",
                Type = "Craftable",
                TextureWidth = 16,
                TextureHeight = 32,
                Variations = _storages.Count,
                EnableContentPatcherCheck = true,
            };
            var textures = new List<Texture2D>();
            var placeholder = Helper.Content.Load<Texture2D>("assets/texture.png");
            foreach (var storageName in _xsLite.API.GetAllStorages().OrderBy(storageName => storageName))
            {
                Texture2D texture = null;
                try
                {
                    texture = Helper.Content.Load<Texture2D>($"ExpandedStorage/SpriteSheets/{storageName}", ContentSource.GameContent);
                }
                catch (Exception)
                {
                    // ignored
                }
                if (texture is null || texture.Width != 80 || texture.Height != 32 && texture.Height != 96)
                    continue;
                textures.Add(placeholder);
                _storages.Add(storageName);
                model.ManualVariations.Add(new VariationModel
                {
                    Id = _storages.Count - 1,
                    Keywords = new List<string> { storageName }
                });
            }
            _alternativeTexturesAPI.AddAlternativeTexture(model, "ExpandedStorage", textures);
        }
        /// <inheritdoc />
        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("AlternativeTextures/Textures/ExpandedStorage.Craftable_Chest");
        }
        /// <inheritdoc />
        public void Edit<T>(IAssetData asset)
        {
            var editor = asset.AsImage();
            editor.ExtendImage(80, _storages.Count * 32);
            for (var i = 0; i < _storages.Count; i++)
            {
                var texture = Helper.Content.Load<Texture2D>($"ExpandedStorage/SpriteSheets/{_storages[i]}", ContentSource.GameContent);
                editor.PatchImage(texture, new Rectangle(0, 0, 16, 32), new Rectangle(0,  i * 32, 16, 32));
                editor.PatchImage(texture, new Rectangle(0, 0, 80, 32), new Rectangle(16,  i * 32, 80, 32));
            }
        }
    }
}