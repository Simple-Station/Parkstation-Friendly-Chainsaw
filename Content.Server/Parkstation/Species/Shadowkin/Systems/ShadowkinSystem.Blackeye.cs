using Content.Server.Nyanotrasen.Cloning;
using Content.Server.Parkstation.Cloning;
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

        SubscribeLocalEvent<BeenClonedEvent>(OnCloned);
    }


    private void OnBlackeyeAttempt(ShadowkinBlackeyeAttemptEvent ev)
    {
        var uid = _entity.GetEntity(ev.Ent);
        if (!_entity.TryGetComponent<ShadowkinComponent>(uid, out var component)
            || component.Blackeye
            || ev.CheckPower
                && component.PowerLevel > ShadowkinComponent.PowerThresholds[ShadowkinPowerThreshold.Min] + 5)
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
        Dirty(uid, component);

        // Remove powers
        _entity.RemoveComponent<ShadowkinDarkSwapPowerComponent>(uid);
        _entity.RemoveComponent<ShadowkinDarkSwappedComponent>(uid);
        _entity.RemoveComponent<ShadowkinRestPowerComponent>(uid);
        _entity.RemoveComponent<ShadowkinTeleportPowerComponent>(uid);
        _entity.RemoveComponent<EmpathyChatComponent>(uid);


        if (!ev.Damage)
            return;

        // Popup
        _popup.PopupEntity(Loc.GetString("shadowkin-blackeye"), uid, uid, PopupType.Large);

        // Stamina crit
        if (_entity.TryGetComponent<StaminaComponent>(uid, out var stamina))
            _stamina.TakeStaminaDamage(uid, stamina.CritThreshold, null, uid);

        // Nearly crit with cellular damage
        // If already 5 damage off of crit, don't do anything
        if (!_entity.TryGetComponent<DamageableComponent>(uid, out var damageable) ||
            !_mobThreshold.TryGetThresholdForState(uid, MobState.Critical, out var key))
            return;

        var minus = damageable.TotalDamage;

        _damageable.TryChangeDamage(uid,
            new DamageSpecifier(_prototype.Index<DamageTypePrototype>("Cellular"),
                Math.Max((double) (key.Value - minus - 5), 0)), true);
    }

    private void OnCloned(BeenClonedEvent ev)
    {
        // Don't give blackeyed Shadowkin their abilities back when they're cloned.
        if (_entity.TryGetComponent<ShadowkinComponent>(ev.OriginalMob, out var shadowkin) &&
            shadowkin.Blackeye)
            _power.TryBlackeye(ev.Mob, false, false);

        // Blackeye the Shadowkin that come from the metempsychosis machine
        if (_entity.HasComponent<MetempsychoticMachineComponent>(ev.Cloner) &&
            _entity.HasComponent<ShadowkinComponent>(ev.Mob))
            _power.TryBlackeye(ev.Mob, false, false);
    }


    /// <summary>
    ///     Tries to blackeye a shadowkin
    /// </summary>
    public bool TryBlackeye(EntityUid uid, bool damage = true, bool checkPower = true)
    {
        if (!_entity.HasComponent<ShadowkinComponent>(uid))
            return false;

        var ent = _entity.GetNetEntity(uid);
        // Raise an attempted blackeye event
        var ev = new ShadowkinBlackeyeAttemptEvent(ent, checkPower);
        RaiseLocalEvent(ev);
        if (ev.Cancelled)
            return false;

        Blackeye(uid, damage);
        return true;
    }

    /// <summary>
    ///     Blackeyes a shadowkin
    /// </summary>
    public void Blackeye(EntityUid uid, bool damage = true)
    {
        var ent = _entity.GetNetEntity(uid);

        // Get shadowkin component
        if (!_entity.TryGetComponent<ShadowkinComponent>(uid, out var component))
        {
            DebugTools.Assert("Tried to blackeye entity without shadowkin component.");
            return;
        }

        component.Blackeye = true;
        RaiseNetworkEvent(new ShadowkinBlackeyeEvent(ent, damage));
        RaiseLocalEvent(new ShadowkinBlackeyeEvent(ent, damage));
    }
}
