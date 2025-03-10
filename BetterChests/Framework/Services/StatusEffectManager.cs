namespace StardewMods.BetterChests.Framework.Services;

using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.Common.Services;
using StardewValley.Buffs;

/// <summary>Responsible for adding or removing custom buffs.</summary>
internal sealed class StatusEffectManager : BaseService<StatusEffectManager>
{
    private readonly IModConfig modConfig;

    /// <summary>Initializes a new instance of the <see cref="StatusEffectManager" /> class.</summary>
    /// <param name="modConfig">Dependency used for managing config data.</param>
    public StatusEffectManager(IModConfig modConfig) => this.modConfig = modConfig;

    /// <summary>Adds a custom status effect to the player.</summary>
    /// <param name="statusEffect">The status effect to add.</param>
    public void AddEffect(StatusEffect statusEffect)
    {
        var buff = this.GetEffect(statusEffect);
        if (buff is null)
        {
            return;
        }

        Log.Trace("Adding effect {0}", statusEffect.ToStringFast());
        Game1.player.buffs.Apply(buff);
    }

    /// <summary>Checks if the specified status effect is currently active on the player.</summary>
    /// <param name="statusEffect">The status effect to check.</param>
    /// <returns><c>true</c> if the status effect is active on the player; otherwise, <c>false</c>.</returns>
    public bool HasEffect(StatusEffect statusEffect)
    {
        var id = this.GetId(statusEffect);
        return !string.IsNullOrWhiteSpace(id) && Game1.player.buffs.IsApplied(id);
    }

    /// <summary>Removes a custom status effect from the player.</summary>
    /// <param name="statusEffect">The status effect to remove.</param>
    public void RemoveEffect(StatusEffect statusEffect)
    {
        var id = this.GetId(statusEffect);
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        Log.Trace("Removing effect {0}", statusEffect.ToStringFast());
        Game1.player.buffs.Remove(id);
    }

    private Buff? GetEffect(StatusEffect statusEffect) =>
        statusEffect switch
        {
            StatusEffect.Overburdened => new Buff(
                this.Prefix + StatusEffect.Overburdened.ToStringFast(),
                displayName: I18n.Effect_CarryChestSlow_Description(),
                duration: 60_000,
                iconTexture: Game1.buffsIcons,
                iconSheetIndex: 13,
                effects: new BuffEffects { Speed = { this.modConfig.CarryChestSlowAmount } }),
            _ => null,
        };

    private string GetId(StatusEffect statusEffect) =>
        statusEffect switch
        {
            StatusEffect.Overburdened => this.Prefix + StatusEffect.Overburdened.ToStringFast(), _ => string.Empty,
        };
}