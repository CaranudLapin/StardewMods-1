namespace StardewMods.BetterChests.Framework.UI.Menus;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

/// <summary>Menu for sorting options.</summary>
internal sealed class SortMenu : SearchMenu
{
    private readonly IStorageContainer container;
    private readonly ClickableTextureComponent copyButton;
    private readonly ClickableTextureComponent okButton;
    private readonly ClickableTextureComponent pasteButton;
    private readonly ClickableTextureComponent saveButton;
    private readonly IExpression? searchExpression;

    /// <summary>Initializes a new instance of the <see cref="SortMenu" /> class.</summary>
    /// <param name="container">The container to categorize.</param>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="reflectionHelper">Dependency used for reflecting into non-public code.</param>
    public SortMenu(
        IStorageContainer container,
        IExpressionHandler expressionHandler,
        IIconRegistry iconRegistry,
        IInputHelper inputHelper,
        IReflectionHelper reflectionHelper)
        : base(expressionHandler, iconRegistry, inputHelper, reflectionHelper, container.SortInventoryBy)
    {
        this.container = container;
        this.searchExpression =
            expressionHandler.TryParseExpression(container.CategorizeChestSearchTerm, out var expression)
                ? expression
                : null;

        this.RefreshItems();

        this.saveButton = iconRegistry
            .RequireIcon(InternalIcon.Save)
            .GetComponent(
                IconStyle.Button,
                this.xPositionOnScreen + this.width + 4,
                this.yPositionOnScreen + Game1.tileSize + 16);

        this.saveButton.hoverText = I18n.Ui_Save_Name();

        this.copyButton = iconRegistry
            .RequireIcon(InternalIcon.Copy)
            .GetComponent(
                IconStyle.Button,
                this.xPositionOnScreen + this.width + 4,
                this.yPositionOnScreen + ((Game1.tileSize + 16) * 2));

        this.copyButton.hoverText = I18n.Ui_Copy_Tooltip();

        this.pasteButton = iconRegistry
            .RequireIcon(InternalIcon.Paste)
            .GetComponent(
                IconStyle.Button,
                this.xPositionOnScreen + this.width + 4,
                this.yPositionOnScreen + ((Game1.tileSize + 16) * 3));

        this.pasteButton.hoverText = I18n.Ui_Paste_Tooltip();

        this.okButton = iconRegistry
            .RequireIcon(VanillaIcon.Ok)
            .GetComponent(
                IconStyle.Transparent,
                this.xPositionOnScreen + this.width + 4,
                this.yPositionOnScreen + this.height - Game1.tileSize - (IClickableMenu.borderWidth / 2));

        this.allClickableComponents.Add(this.saveButton);
        this.allClickableComponents.Add(this.copyButton);
        this.allClickableComponents.Add(this.pasteButton);
        this.allClickableComponents.Add(this.okButton);
    }

    /// <inheritdoc />
    protected override List<Item> GetItems()
    {
        var items = this.searchExpression is null
            ? ItemRepository.GetItems().ToList()
            : ItemRepository.GetItems(this.searchExpression.Equals).ToList();

        if (this.Expression is not null)
        {
            items.Sort(this.Expression);
        }

        return items;
    }

    /// <inheritdoc />
    protected override bool HighlightMethod(Item item) => true;

    /// <inheritdoc />
    protected override bool TryLeftClick(Point cursor)
    {
        if (this.saveButton.bounds.Contains(cursor) && this.readyToClose())
        {
            Game1.playSound("drumkit6");
            this.container.SortInventoryBy = this.SearchText;
            return true;
        }

        if (this.copyButton.bounds.Contains(cursor))
        {
            Game1.playSound("drumkit6");
            DesktopClipboard.SetText(this.SearchText);
            return true;
        }

        if (this.pasteButton.bounds.Contains(cursor))
        {
            Game1.playSound("drumkit6");
            var searchText = string.Empty;
            DesktopClipboard.GetText(ref searchText);
            this.SetSearchText(searchText, true);
            return true;
        }

        if (this.okButton.bounds.Contains(cursor))
        {
            Game1.playSound("bigDeSelect");
            this.exitThisMenuNoSound();
            this.container.ShowMenu();
            return true;
        }

        return false;
    }
}