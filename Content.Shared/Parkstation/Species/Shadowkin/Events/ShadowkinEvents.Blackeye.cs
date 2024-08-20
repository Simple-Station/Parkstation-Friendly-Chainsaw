using Robust.Shared.Serialization;

namespace Content.Shared.Parkstation.Species.Shadowkin.Events;

/// <summary>
///     Raised to notify other systems of an attempt to blackeye a shadowkin.
/// </summary>
public sealed class ShadowkinBlackeyeAttemptEvent(NetEntity ent, bool checkPower = true) : CancellableEntityEventArgs
{
    public readonly NetEntity Ent = ent;
    public readonly bool CheckPower = checkPower;
}

/// <summary>
///     Raised when a shadowkin becomes a blackeye.
/// </summary>
[Serializable, NetSerializable]
public sealed class ShadowkinBlackeyeEvent(NetEntity ent, bool damage = true) : EntityEventArgs
{
    public readonly NetEntity Ent = ent;
    public readonly bool Damage = damage;
}
