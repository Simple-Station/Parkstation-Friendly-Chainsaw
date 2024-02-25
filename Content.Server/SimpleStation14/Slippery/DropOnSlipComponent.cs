namespace Content.Server.SimpleStation14.Slippery;

/// <summary>
///     Uses provided chance to try and drop the item when slipped, if equipped.
/// </summary>
[RegisterComponent]
public sealed partial class DropOnSlipComponent : Component
{
    [DataField("chance")]
    [ViewVariables(VVAccess.ReadWrite)]
    public int Chance = 20;

    [DataField("chanceToThrow")]
    [ViewVariables(VVAccess.ReadWrite)]
    public int ChanceToThrow = 40;
}
