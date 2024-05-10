namespace StardewMods.FauxCore.Framework.Models.Expressions;

using System.ComponentModel;
using StardewMods.Common.Enums;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Inventories;

/// <summary>Represents an item attribute term.</summary>
internal sealed class DynamicTerm : IExpression
{
    /// <summary>The begin attribute character.</summary>
    public const char BeginChar = '{';

    /// <summary>The end attribute character.</summary>
    public const char EndChar = '}';

    private static readonly Dictionary<ItemAttribute, Func<Item, object>> Accessors = new()
    {
        { ItemAttribute.Category, item => item.getCategoryName() },
        { ItemAttribute.Name, item => item.DisplayName },
        { ItemAttribute.Quantity, item => item.Stack },
        { ItemAttribute.Quality, item => item.Quality },
        { ItemAttribute.Tags, item => item.GetContextTags() },
    };

    private readonly ItemAttribute attribute;

    /// <summary>Initializes a new instance of the <see cref="DynamicTerm" /> class.</summary>
    /// <param name="expression">The expression.</param>
    public DynamicTerm(string expression) =>
        this.attribute = ItemAttributeExtensions.TryParse(expression, out var itemAttribute, true)
            ? itemAttribute
            : throw new InvalidEnumArgumentException($"Invalid item attribute: {expression}");

    /// <inheritdoc />
    public IEnumerable<IExpression> Expressions => Array.Empty<IExpression>();

    /// <inheritdoc />
    public ExpressionType ExpressionType => ExpressionType.Dynamic;

    /// <inheritdoc />
    public string Term => this.attribute.ToStringFast();

    /// <inheritdoc />
    public int Compare(Item? x, Item? y)
    {
        if (x is null && y is null)
        {
            return 0;
        }

        if (x is null || !this.TryGetValue(x, out var xValue))
        {
            return -1;
        }

        if (y is null || !this.TryGetValue(y, out var yValue))
        {
            return 1;
        }

        if (xValue is string xString && yValue is string yString)
        {
            return string.Compare(xString, yString, StringComparison.OrdinalIgnoreCase);
        }

        if (xValue is int xInt && yValue is int yInt)
        {
            return xInt.CompareTo(yInt);
        }

        return 0;
    }

    /// <inheritdoc />
    public bool Equals(Item? item) => true;

    /// <inheritdoc />
    public bool Equals(IInventory? other) => true;

    /// <inheritdoc />
    public bool Equals(string? other) => true;

    /// <inheritdoc />
    public override string ToString() => $"{DynamicTerm.BeginChar}{this.attribute.ToStringFast()}{DynamicTerm.EndChar}";

    /// <summary>Tries to retrieve the attribute value.</summary>
    /// <param name="item">The item from which to retrieve the value.</param>
    /// <param name="value">When this method returns, contains the attribute value; otherwise, default.</param>
    /// <returns><c>true</c> if the value was successfully retrieved; otherwise, <c>false</c>.</returns>
    public bool TryGetValue(Item? item, [NotNullWhen(true)] out object? value)
    {
        if (item is null || !DynamicTerm.Accessors.TryGetValue(this.attribute, out var accessor))
        {
            value = null;
            return false;
        }

        value = accessor(item);
        return true;
    }

    /// <summary>Tries to parse the value into an int.</summary>
    /// <param name="value">The value to parse.</param>
    /// <param name="result">When this method returns, contains the parsed value; otherwise, default.</param>
    /// <returns><c>true</c> if the value was successfully parsed; otherwise, <c>false</c>.</returns>
    public bool TryParse(string value, [NotNullWhen(true)] out int? result)
    {
        switch (this.attribute)
        {
            case ItemAttribute.Quantity when int.TryParse(value, out var intValue):
                result = intValue;
                return true;
            case ItemAttribute.Quality when ItemQualityExtensions.TryParse(value, out var itemQuality, true):
                result = (int)itemQuality;
                return true;
            case ItemAttribute.Quality:
                result = (int)ItemQualityExtensions
                    .GetValues()
                    .FirstOrDefault(
                        itemQuality => itemQuality.ToStringFast().Contains(value, StringComparison.OrdinalIgnoreCase));

                return true;
            default:
                result = null;
                return false;
        }
    }
}