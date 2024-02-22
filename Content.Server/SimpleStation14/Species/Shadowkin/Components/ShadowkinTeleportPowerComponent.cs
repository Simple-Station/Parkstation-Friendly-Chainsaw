using Robust.Shared.GameStates;

namespace Content.Server.SimpleStation14.Species.Shadowkin.Components;

[RegisterComponent]
public sealed partial class ShadowkinTeleportPowerComponent : Component
{
    [DataField("shadowkinTeleportActionEntity")]
    public EntityUid? ShadowkinTeleportActionEntity;
}
