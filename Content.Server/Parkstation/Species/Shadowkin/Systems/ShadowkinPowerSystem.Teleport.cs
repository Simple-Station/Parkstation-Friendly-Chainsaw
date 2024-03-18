using Content.Server.Magic;
using Content.Server.Parkstation.Species.Shadowkin.Components;
using Content.Server.Pulling;
using Content.Shared.Actions;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Parkstation.Species.Shadowkin.Events;
using Content.Shared.Pulling.Components;
using Content.Shared.Parkstation.Species.Shadowkin.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server.Parkstation.Species.Shadowkin.Systems;

public sealed class ShadowkinTeleportSystem : EntitySystem
{
    [Dependency] private readonly ShadowkinPowerSystem _power = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly MagicSystem _magic = default!;

    [ValidatePrototypeId<EntityPrototype>]
    private const string ShadowkinTeleportActionId = "ShadowkinTeleportAction";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowkinTeleportPowerComponent, ComponentStartup>(Startup);
        SubscribeLocalEvent<ShadowkinTeleportPowerComponent, ComponentShutdown>(Shutdown);

        SubscribeLocalEvent<ShadowkinTeleportPowerComponent, ShadowkinTeleportEvent>(Teleport);
    }


    private void Startup(EntityUid uid, ShadowkinTeleportPowerComponent component, ComponentStartup args)
    {
        _actions.AddAction(uid, ref component.ShadowkinTeleportActionEntity, ShadowkinTeleportActionId);
    }

    private void Shutdown(EntityUid uid, ShadowkinTeleportPowerComponent component, ComponentShutdown args)
    {
        _actions.RemoveAction(uid, component.ShadowkinTeleportActionEntity);
    }


    private void Teleport(EntityUid uid, ShadowkinTeleportPowerComponent component, ShadowkinTeleportEvent args)
    {
        // Need power to drain power
        if (!_entity.TryGetComponent<ShadowkinComponent>(args.Performer, out var comp))
            return;

        // Don't activate abilities if handcuffed
        // TODO: Something like the Psionic Headcage to disable powers for Shadowkin
        if (_entity.HasComponent<HandcuffComponent>(args.Performer))
            return;


        var transform = Transform(args.Performer);
        if (transform.MapID != args.Target.GetMapId(EntityManager))
            return;

        SharedPullableComponent? pullable = null; // To avoid "might not be initialized when accessed" warning
        if (_entity.TryGetComponent<SharedPullerComponent>(args.Performer, out var puller) &&
            puller.Pulling != null &&
            _entity.TryGetComponent<SharedPullableComponent>(puller.Pulling, out pullable) &&
            pullable.BeingPulled)
        {
            // Temporarily stop pulling to avoid not teleporting to the target
            _pulling.TryStopPull(pullable);
        }

        // Teleport the performer to the target
        _transform.SetCoordinates(args.Performer, args.Target);
        transform.AttachToGridOrMap();

        if (pullable != null && puller != null)
        {
            // Get transform of the pulled entity
            var pulledTransform = Transform(pullable.Owner);

            // Teleport the pulled entity to the target
            // TODO: Relative position to the performer
            _transform.SetCoordinates(pullable.Owner, args.Target);
            pulledTransform.AttachToGridOrMap();

            // Resume pulling
            // TODO: This does nothing? // This does things sometimes, but the client never knows // This does nothing??
            _pulling.TryStartPull(puller, pullable);
        }


        // Play the teleport sound
        _audio.PlayPvs(args.Sound, args.Performer, AudioParams.Default.WithVolume(args.Volume));

        // Take power and deal stamina damage
        _power.TryAddPowerLevel(comp.Owner, -args.PowerCost);
        _stamina.TakeStaminaDamage(args.Performer, args.StaminaCost);

        // Speak
        _magic.Speak(args, false);

        args.Handled = true;
    }
}
