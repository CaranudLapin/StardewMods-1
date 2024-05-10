namespace StardewMods.Common.Services.Integrations.FauxCore;

using NetEscapades.EnumGenerators;

[EnumExtensions]
public enum ExpressionType
{
    /// <summary>An expression in which all sub-expressions must be true.</summary>
    All,
    
    /// <summary>An expression in which any sub-expression must be true.</summary>
    Any,
    
    /// <summary>An expression where the first sub-expression must match the second.</summary>
    Comparable,
    
    /// <summary>An expression that dynamically pulls an attribute from the Item.</summary>
    Dynamic,

    /// <summary>An expression where its sub-expression must not be true.</summary>
    Not,
    
    /// <summary>An expression that matches an Item's internal attributes against the string.</summary>
    Static,
}
