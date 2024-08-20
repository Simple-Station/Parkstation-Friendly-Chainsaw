using System.Linq;
using Content.Server.Magic;
using Content.Server.NPC.Components;
using Content.Server.NPC.Systems;
using Content.Server.Parkstation.Species.Shadowkin.Components;
using Content.Shared.Actions;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Eye;
using Content.Shared.Ghost;
using Content.Shared.Parkstation.Species.Shadowkin.Events;
using Content.Shared.Parkstation.Species.Shadowkin.Components;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server.Parkstation.Species.Shadowkin.Systems;

public sealed class ShadowkinDarkSwapSystem : EntitySystem
{
    [Dependency] private readonly ShadowkinPowerSystem _power = default!;
    [Dependency] private readonly VisibilitySystem _visibility = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly ShadowkinDarkenSystem _darken = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly SharedStealthSystem _stealth = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly MagicSystem _magic = default!;
    [Dependency] private readonly NpcFactionSystem _factions = default!;

    [ValidatePrototypeId<EntityPrototype>]
    private const string ShadowkinDarkSwapActionId = "ShadowkinDarkSwapAction";


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowkinDarkSwapPowerComponent, ComponentStartup>(Startup);
        SubscribeLocalEvent<ShadowkinDarkSwapPowerComponent, ComponentShutdown>(Shutdown);

        SubscribeLocalEvent<ShadowkinDarkSwapPowerComponent, ShadowkinDarkSwapEvent>(DarkSwap);

        SubscribeLocalEvent<ShadowkinDarkSwappedComponent, ComponentStartup>(OnInvisStartup);
        SubscribeLocalEvent<ShadowkinDarkSwappedComponent, ComponentShutdown>(OnInvisShutdown);
    }


    private void Startup(EntityUid uid, ShadowkinDarkSwapPowerComponent component, ComponentStartup args)
    {
        _actions.AddAction(uid, ref component.ShadowkinDarkSwapActionEntity, ShadowkinDarkSwapActionId, uid);
    }

    private void Shutdown(EntityUid uid, ShadowkinDarkSwapPowerComponent component, ComponentShutdown args)
    {
        _actions.RemoveAction(uid, component.ShadowkinDarkSwapActionEntity);
    }

    private void OnInvisStartup(EntityUid uid, ShadowkinDarkSwappedComponent component, ComponentStartup args)
    {
        if (component.Pacify)
            EnsureComp<PacifiedComponent>(uid);

        if (!component.Invisible)
            return;

        SetVisibility(uid, true);
        SuppressFactions(uid, true);
    }

    private void OnInvisShutdown(EntityUid uid, ShadowkinDarkSwappedComponent component, ComponentShutdown args)
    {
        RemComp<PacifiedComponent>(uid);

        if (component.Invisible)
        {
            SetVisibility(uid, false);
            SuppressFactions(uid, false);
        }

        component.Darken = false;

        foreach (var light in component.DarkenedLights.ToArray())
        {
            if (!_entity.TryGetComponent<PointLightComponent>(light, out var pointLight) ||
                !_entity.TryGetComponent<ShadowkinLightComponent>(light, out var shadowkinLight))
                continue;

            _darken.ResetLight(pointLight, shadowkinLight);
        }

        component.DarkenedLights.Clear();
    }


    private void DarkSwap(EntityUid uid, ShadowkinDarkSwapPowerComponent component, ShadowkinDarkSwapEvent args)
    {
        var performer = _entity.GetNetEntity(args.Performer);

        // Need power to drain power
        if (!_entity.HasComponent<ShadowkinComponent>(args.Performer))
            return;

        // Don't activate abilities if handcuffed
        // TODO: Something like the Psionic Headcage to disable powers for Shadowkin
        if (_entity.HasComponent<HandcuffComponent>(args.Performer))
            return;


        var hasComp = _entity.HasComponent<ShadowkinDarkSwappedComponent>(args.Performer);

        if (hasComp)
            Darkened(performer, args.StaminaCostOff, args.PowerCostOff, args.SoundOff, args.VolumeOff, args);
        else
            UnDarkened(performer, args.StaminaCostOn, args.PowerCostOn, args.SoundOn, args.VolumeOn, args);

        _magic.Speak(args, false);
    }

    public void Darkened(NetEntity performer, float staminaCostOff, float powerCostOff, SoundSpecifier soundOff, float volumeOff, ShadowkinDarkSwapEvent? args)
    {
        var performerUid = _entity.GetEntity(performer);

        var ev = new ShadowkinDarkSwapAttemptEvent(performerUid);
        RaiseLocalEvent(ev);
        if (ev.Cancelled)
            return;

        _entity.RemoveComponent<ShadowkinDarkSwappedComponent>(performerUid);
        RaiseNetworkEvent(new ShadowkinDarkSwappedEvent(performer, false));

        _audio.PlayPvs(soundOff, performerUid, AudioParams.Default.WithVolume(volumeOff));

        _power.TryAddPowerLevel(performerUid, -powerCostOff);
        _stamina.TakeStaminaDamage(performerUid, staminaCostOff);

        if (args != null)
            args.Handled = true;
    }

    public void UnDarkened(NetEntity performer, float staminaCostOn, float powerCostOn, SoundSpecifier soundOn, float volumeOn, ShadowkinDarkSwapEvent? args)
    {
        var performerUid = _entity.GetEntity(performer);

        var ev = new ShadowkinDarkSwapAttemptEvent(performerUid);
        RaiseLocalEvent(ev);
        if (ev.Cancelled)
            return;

        var comp = _entity.EnsureComponent<ShadowkinDarkSwappedComponent>(performerUid);
        comp.Invisible = true;
        comp.Pacify = true;
        comp.Darken = true;

        RaiseNetworkEvent(new ShadowkinDarkSwappedEvent(performer, true));

        _audio.PlayPvs(soundOn, performerUid, AudioParams.Default.WithVolume(volumeOn));

        _power.TryAddPowerLevel(performerUid, -powerCostOn);
        _stamina.TakeStaminaDamage(performerUid, staminaCostOn);

        if (args != null)
            args.Handled = true;
    }

    public void SetVisibility(EntityUid uid, bool set)
    {
        // We require the visibility component for this to work
        EnsureComp<VisibilityComponent>(uid);

        // Allow ghosts to see DarkSwapped entities
        if (_entity.HasComponent<GhostComponent>(uid))
        {
            if (_entity.TryGetComponent(uid, out EyeComponent? eye))
                _eye.SetVisibilityMask(uid, eye.VisibilityMask | (int) VisibilityFlags.DarkSwapInvisibility, eye);

            return;
        }

        if (set) // Invisible
        {
            // Allow the entity to see DarkSwapped entities
            if (_entity.TryGetComponent(uid, out EyeComponent? eye))
                _eye.SetVisibilityMask(uid, eye.VisibilityMask | (int) VisibilityFlags.DarkSwapInvisibility, eye);

            // Make other entities unable to see the entity unless also DarkSwapped
            _visibility.AddLayer(uid, (ushort) VisibilityFlags.DarkSwapInvisibility, false);
            _visibility.RemoveLayer(uid, (ushort) VisibilityFlags.Normal, false);
            _visibility.RefreshVisibility(uid);

            // Add a stealth shader to the entity
            _stealth.SetVisibility(uid, 0.8f, _entity.EnsureComponent<StealthComponent>(uid));
        }
        else // Visible
        {
            // Remove the ability to see DarkSwapped entities
            if (_entity.TryGetComponent(uid, out EyeComponent? eye))
                _eye.SetVisibilityMask(uid, eye.VisibilityMask & ~(int) VisibilityFlags.DarkSwapInvisibility, eye);

            // Make other entities able to see the entity again
            _visibility.RemoveLayer(uid, (ushort) VisibilityFlags.DarkSwapInvisibility, false);
            _visibility.AddLayer(uid, (ushort) VisibilityFlags.Normal, false);
            _visibility.RefreshVisibility(uid);

            // Remove the stealth shader from the entity
            _stealth.SetVisibility(uid, 1f, _entity.EnsureComponent<StealthComponent>(uid));
        }
    }

    /// <summary>
    ///     Remove existing factions on the entity and move them to the power component to add back when removed from The Dark
    /// </summary>
    /// <param name="uid">Entity to modify factions for</param>
    /// <param name="set">Add or remove the factions</param>
    public void SuppressFactions(EntityUid uid, bool set)
    {
        // We require the power component to keep track of the factions
        if (!_entity.TryGetComponent<ShadowkinDarkSwapPowerComponent>(uid, out var component))
            return;

        if (set)
        {
            if (!_entity.TryGetComponent<NpcFactionMemberComponent>(uid, out var factions))
                return;

            // Copy the suppressed factions to the power component
            component.SuppressedFactions = factions.Factions.ToList();

            // Remove the factions from the entity
            foreach (var faction in factions.Factions)
                _factions.RemoveFaction(uid, faction);

            // Add status factions for The Dark to the entity
            foreach (var faction in component.AddedFactions)
                _factions.AddFaction(uid, faction);
        }
        else
        {
            // Remove the status factions from the entity
            foreach (var faction in component.AddedFactions)
                _factions.RemoveFaction(uid, faction);

            // Add the factions back to the entity
            foreach (var faction in component.SuppressedFactions)
                _factions.AddFaction(uid, faction);

            component.SuppressedFactions.Clear();
        }
    }

    public void ForceDarkSwap(EntityUid uid, ShadowkinComponent component)
    {
        // Add/Remove the component, which should handle the rest
        if (_entity.HasComponent<ShadowkinDarkSwappedComponent>(uid))
            _entity.RemoveComponent<ShadowkinDarkSwappedComponent>(uid);
        else
            _entity.AddComponent<ShadowkinDarkSwappedComponent>(uid);
    }
}
