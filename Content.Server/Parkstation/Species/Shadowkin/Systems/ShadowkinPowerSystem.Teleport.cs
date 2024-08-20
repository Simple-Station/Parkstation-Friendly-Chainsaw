using System.Numerics;
using Content.Server.Magic;
using Content.Server.Parkstation.Species.Shadowkin.Components;
using Content.Shared.Actions;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Interaction;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Parkstation.Species.Shadowkin.Events;
using Content.Shared.Parkstation.Species.Shadowkin.Components;
using Content.Shared.Physics;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Parkstation.Species.Shadowkin.Systems;

public sealed class ShadowkinTeleportSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ShadowkinPowerSystem _power = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly MagicSystem _magic = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;

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
        if (!_entity.TryGetComponent<ShadowkinComponent>(args.Performer, out var comp)
            // Don't activate abilities if handcuffed
            || _entity.TryGetComponent<HandcuffComponent>(args.Performer, out var cuff)
                && cuff.AntiShadowkin)
            return;


        var transform = Transform(args.Performer);
        // Must be on the same map
        if (transform.MapID != args.Target.GetMapId(EntityManager))
            return;

        PullableComponent? pullable = null; // To avoid "might not be initialized when accessed" warning
        if (_entity.TryGetComponent<PullerComponent>(args.Performer, out var puller) &&
            puller.Pulling != null &&
            _entity.TryGetComponent(puller.Pulling, out pullable) &&
            pullable.BeingPulled)
        {
            // Temporarily stop pulling to avoid not teleporting to the target
            _pulling.TryStopPull(puller.Pulling.Value, pullable);
        }

        // Teleport the performer to the target
        _transform.SetCoordinates(args.Performer, args.Target);
        _transform.AttachToGridOrMap(args.Performer);

        if (pullable != null && puller != null)
        {
            // Get transform of the pulled entity
            var pulledTransform = Transform(pullable.Owner);

            // Teleport the pulled entity to the target
            // TODO: Relative position to the performer
            _transform.SetCoordinates(pullable.Owner, args.Target);
            _transform.AttachToGridOrMap(pullable.Owner);

            // Resume pulling
            _pulling.TryStartPull(args.Performer, pullable.Owner);
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


    public void ForceTeleport(EntityUid uid, ShadowkinComponent component)
    {
        // Create the event we'll later raise, and set it to our Shadowkin.
        var args = new ShadowkinTeleportEvent { Performer = uid };

        // Pick a random location on the map until we find one that can be reached.
        var coords = Transform(uid).Coordinates;
        EntityCoordinates? target = null;

        // It'll iterate up to 8 times, shrinking in distance each time, and if it doesn't find a valid location, it'll return.
        for (var i = 8; i != 0; i--)
        {
            var angle = Angle.FromDegrees(_random.Next(360));
            var offset = new Vector2((float) (i * Math.Cos(angle)), (float) (i * Math.Sin(angle)));

            target = coords.Offset(offset);

            if (_interaction.InRangeUnobstructed(uid, target.Value, 0,
                    CollisionGroup.MobMask | CollisionGroup.MobLayer))
                break;

            target = null;
        }

        // If we didn't find a valid location, return.
        if (target == null)
            return;

        args.Target = target.Value;

        // Raise the event to teleport the Shadowkin.
        RaiseLocalEvent(uid, args);
    }
}
