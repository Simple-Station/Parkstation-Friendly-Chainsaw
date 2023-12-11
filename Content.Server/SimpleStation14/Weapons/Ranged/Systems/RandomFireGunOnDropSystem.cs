using Content.Server.SimpleStation14.Weapons.Ranged.Components;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Random;
using Serilog;

namespace Content.Server.SimpleStation14.Weapons.Ranged.Systems;

public sealed class RandomFireGunOnDropSystem : EntitySystem
{
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IEntityManager _entity = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RandomFireGunOnDropComponent, LandEvent>(HandleLand);
    }


    private void HandleLand(EntityUid uid, RandomFireGunOnDropComponent component, ref LandEvent args)
    {
        Log.Warning($"{args.User} firing {uid}");

        if (!_entity.TryGetComponent<GunComponent>(uid, out var gun) ||
            args.User == null)
            return;

        if (_random.Prob(component.FireOnDropChance))
            // The gun fires itself (weird), with the target being its own position offset by its rotation as a point vector.
            // The result being that it will always fire the direction that all gun sprites point in.
            _gun.AttemptShoot(uid, uid, gun, Transform(uid).Coordinates.Offset(Transform(uid).LocalRotation.ToVec()));
    }
}
