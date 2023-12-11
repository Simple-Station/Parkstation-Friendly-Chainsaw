namespace Content.Server.SimpleStation14.Weapons.Ranged.Components;

[RegisterComponent]
public sealed partial class RandomFireGunOnDropComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float FireOnDropChance = 0.1f;
}
