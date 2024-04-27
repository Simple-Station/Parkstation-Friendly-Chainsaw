using Content.Server.Parkstation.Species.Shadowkin.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Parkstation.Species.Shadowkin.Components;
using Content.Shared.Parkstation.Species.Shadowkin.Events;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.Parkstation.Species.Shadowkin.Systems;

public sealed class ShadowkinBlackeyeSystem : EntitySystem
{
    [Dependency] private readonly ShadowkinPowerSystem _power = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowkinBlackeyeAttemptEvent>(OnBlackeyeAttempt);
        SubscribeAllEvent<ShadowkinBlackeyeEvent>(OnBlackeye);
    }


    private void OnBlackeyeAttempt(ShadowkinBlackeyeAttemptEvent ev)
    {
        var uid = _entity.GetEntity(ev.Ent);
        if (!_entity.TryGetComponent<ShadowkinComponent>(uid, out var component) ||
            component.Blackeye ||
            !(component.PowerLevel <= ShadowkinComponent.PowerThresholds[ShadowkinPowerThreshold.Min] + 5))
            ev.Cancel();
    }

    private void OnBlackeye(ShadowkinBlackeyeEvent ev)
    {
        var uid = _entity.GetEntity(ev.Ent);

        // Check if the entity is a shadowkin
        if (!_entity.TryGetComponent<ShadowkinComponent>(uid, out var component))
            return;

        // Stop gaining power
        component.Blackeye = true;
        component.PowerLevelGainEnabled = false;
        _power.SetPowerLevel(uid, ShadowkinComponent.PowerThresholds[ShadowkinPowerThreshold.Min]);

        // Update client state
        Dirty(component);

        // Remove powers
        _entity.RemoveComponent<ShadowkinDarkSwapPowerComponent>(uid);
        _entity.RemoveComponent<ShadowkinDarkSwappedComponent>(uid);
        _entity.RemoveComponent<ShadowkinRestPowerComponent>(uid);
        _entity.RemoveComponent<ShadowkinTeleportPowerComponent>(uid);


        if (!ev.Damage)
            return;

        // Popup
        _popup.PopupEntity(Loc.GetString("shadowkin-blackeye"), uid, uid, PopupType.Large);

        // Stamina crit
        if (_entity.TryGetComponent<StaminaComponent>(uid, out var stamina))
        {
            _stamina.TakeStaminaDamage(uid, stamina.CritThreshold, null, uid);
        }

        // Nearly crit with cellular damage
        // If already 5 damage off of crit, don't do anything
        if (!_entity.TryGetComponent<DamageableComponent>(uid, out var damageable) ||
            !_mobThreshold.TryGetThresholdForState(uid, MobState.Critical, out var key))
            return;

        var minus = damageable.TotalDamage;

        _damageable.TryChangeDamage(uid,
            new DamageSpecifier(_prototype.Index<DamageTypePrototype>("Cellular"),
                Math.Max((double) (key.Value - minus - 5), 0)),
            true,
            true,
            null,
            null);
    }


    /// <summary>
    ///     Tries to blackeye a shadowkin
    /// </summary>
    public bool TryBlackeye(EntityUid uid)
    {
        var ent = _entity.GetNetEntity(uid);
        // Raise an attempted blackeye event
        var ev = new ShadowkinBlackeyeAttemptEvent(ent);
        RaiseLocalEvent(ev);
        if (ev.Cancelled)
            return false;

        Blackeye(uid);
        return true;
    }

    /// <summary>
    ///     Blackeyes a shadowkin
    /// </summary>
    public void Blackeye(EntityUid uid)
    {
        var ent = _entity.GetNetEntity(uid);

        // Get shadowkin component
        if (!_entity.TryGetComponent<ShadowkinComponent>(uid, out var component))
        {
            DebugTools.Assert("Tried to blackeye entity without shadowkin component.");
            return;
        }

        component.Blackeye = true;
        RaiseNetworkEvent(new ShadowkinBlackeyeEvent(ent));
        RaiseLocalEvent(new ShadowkinBlackeyeEvent(ent));
    }
}
