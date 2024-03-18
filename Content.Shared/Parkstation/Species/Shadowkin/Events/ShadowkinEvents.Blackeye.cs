using Robust.Shared.Serialization;

namespace Content.Shared.Parkstation.Species.Shadowkin.Events;

/// <summary>
///     Raised to notify other systems of an attempt to blackeye a shadowkin.
/// </summary>
public sealed class ShadowkinBlackeyeAttemptEvent : CancellableEntityEventArgs
{
    public readonly NetEntity Ent;

    public ShadowkinBlackeyeAttemptEvent(NetEntity ent)
    {
        Ent = ent;
    }
}

/// <summary>
///     Raised when a shadowkin becomes a blackeye.
/// </summary>
[Serializable, NetSerializable]
public sealed class ShadowkinBlackeyeEvent : EntityEventArgs
{
    public readonly NetEntity Ent;
    public readonly bool Damage;

    public ShadowkinBlackeyeEvent(NetEntity ent, bool damage = true)
    {
        Ent = ent;
        Damage = damage;
    }
}
