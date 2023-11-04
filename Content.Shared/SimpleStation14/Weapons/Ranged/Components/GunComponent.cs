// ReSharper disable once CheckNamespace // Extending the GunComponent to add a variable
namespace Content.Shared.Weapons.Ranged.Components;

public partial class GunComponent
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float FireOnDropChance = 0.1f;
}
