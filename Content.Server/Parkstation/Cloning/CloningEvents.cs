using Content.Shared.Mind;
using Content.Shared.Preferences;

namespace Content.Server.Parkstation.Cloning;

[ByRefEvent]
public sealed class BeingClonedEvent(HumanoidCharacterProfile profile, MindComponent mind, EntityUid cloner) : CancellableEntityEventArgs
{
    public HumanoidCharacterProfile Profile = profile;
    public MindComponent Mind = mind;
    public EntityUid Cloner = cloner;
}

public sealed class BeenClonedEvent(HumanoidCharacterProfile profile, MindComponent mind, EntityUid mob, EntityUid OriginalMob, EntityUid cloner) : EntityEventArgs
{
    public HumanoidCharacterProfile Profile = profile;
    public MindComponent Mind = mind;
    public EntityUid Mob = mob;
    public EntityUid OriginalMob = OriginalMob;
    public EntityUid Cloner = cloner;
}
