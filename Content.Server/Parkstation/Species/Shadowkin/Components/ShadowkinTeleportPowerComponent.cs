namespace Content.Server.Parkstation.Species.Shadowkin.Components;

[RegisterComponent]
public sealed partial class ShadowkinTeleportPowerComponent : Component
{
    [DataField("shadowkinTeleportActionEntity")]
    public EntityUid? ShadowkinTeleportActionEntity;
}
