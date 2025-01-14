namespace StardewMods.BetterChests.Framework.UI.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.BetterChests.Framework.Services.Features;
using StardewMods.Common.UI.Menus;
using StardewValley.Menus;

/// <summary>Menu for accessing debug mode.</summary>
internal sealed class DebugMenu : BaseMenu
{
    private readonly List<Rectangle> areas;
    private readonly DebugMode debugMode;
    private readonly List<string> descriptions;
    private readonly List<string> items;

    /// <summary>Initializes a new instance of the <see cref="DebugMenu" /> class.</summary>
    /// <param name="debugMode">Dependency used for debugging features.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    public DebugMenu(DebugMode debugMode, IInputHelper inputHelper)
        : base(inputHelper)
    {
        this.debugMode = debugMode;
        var lineHeight = Game1.smallFont.MeasureString("T").ToPoint().Y;
        this.items = ["backpack", "reset", "config", "layout", "search", "sort", "tab"];
        this.descriptions =
        [
            "Configure the player backpack",
            "Reset individual storages to default",
            "Open the config menu",
            "Open the layout menu",
            "Open the search menu",
            "Open the sort menu",
            "Open the tab menu",
        ];

        this.areas = this
            .items.Select(
                (_, i) => new Rectangle(
                    this.xPositionOnScreen
                    + IClickableMenu.spaceToClearSideBorder
                    + (IClickableMenu.borderWidth / 2)
                    + 16,
                    this.yPositionOnScreen
                    + IClickableMenu.spaceToClearSideBorder
                    + (IClickableMenu.borderWidth / 2)
                    + Game1.tileSize
                    + (i * lineHeight)
                    + 12,
                    250,
                    lineHeight))
            .ToList();
    }

    /// <inheritdoc />
    protected override void Draw(SpriteBatch spriteBatch)
    {
        var hoverText = string.Empty;
        var (mouseX, mouseY) = Game1.getMousePosition(true);

        for (var i = 0; i < this.items.Count; i++)
        {
            var item = this.items[i];
            var area = this.areas[i];
            spriteBatch.DrawString(Game1.smallFont, item, new Vector2(area.X, area.Y), Game1.textColor);
            if (area.Contains(mouseX, mouseY))
            {
                hoverText = this.descriptions[i];
            }
        }

        if (!string.IsNullOrWhiteSpace(hoverText))
        {
            IClickableMenu.drawHoverText(spriteBatch, hoverText, Game1.smallFont);
        }

        Game1.mouseCursorTransparency = 1f;
        this.drawMouse(spriteBatch);
    }

    /// <inheritdoc />
    protected override bool TryLeftClick(int x, int y)
    {
        for (var i = 0; i < this.items.Count; i++)
        {
            var item = this.items[i];
            var area = this.areas[i];
            if (area.Contains(x, y))
            {
                switch (item)
                {
                    case "backpack":
                        this.debugMode.Command("bc_config", [item]);
                        return true;
                    case "reset":
                        this.debugMode.Command("bc_reset", [item]);
                        return true;
                    case "config":
                    case "layout":
                    case "search":
                    case "sort":
                    case "tab":
                        this.debugMode.Command("bc_menu", [item]);
                        return true;
                }
            }
        }

        return false;
    }
}