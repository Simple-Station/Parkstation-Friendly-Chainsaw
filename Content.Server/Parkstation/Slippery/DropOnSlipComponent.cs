namespace Content.Server.Parkstation.Slippery;

/// <summary>
///     Uses provided chance to try and drop the item when slipped, if equipped.
/// </summary>
[RegisterComponent]
public sealed partial class DropOnSlipComponent : Component
{
    /// <summary>
    ///     Percent chance to drop this item when slipping
    /// </summary>
    [DataField("chance")]
    [ViewVariables(VVAccess.ReadWrite)]
    public int Chance = 20;
}
