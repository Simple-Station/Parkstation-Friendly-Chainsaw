namespace Content.Server.Parkstation.Species.Shadowkin.Components;

[RegisterComponent]
public sealed partial class ShadowkinRestPowerComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public bool IsResting = false;

    [DataField("shadowkinRestActionEntity")]
    public EntityUid? ShadowkinRestActionEntity;
}
