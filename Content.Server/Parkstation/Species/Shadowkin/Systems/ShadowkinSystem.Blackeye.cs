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
using Content.Shared.SimpleStation14.Species.Shadowkin.Components;
using Robust.Shared.Prototypes;
using ShadowkinComponent = Content.Shared.Parkstation.Species.Shadowkin.Components.ShadowkinComponent;
using ShadowkinDarkSwappedComponent = Content.Shared.Parkstation.Species.Shadowkin.Components.ShadowkinDarkSwappedComponent;

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
}
