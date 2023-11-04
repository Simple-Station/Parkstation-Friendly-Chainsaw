using Content.Shared.Throwing;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Random;

namespace Content.Shared.SimpleStation14.Weapons.Ranged.Systems;

public sealed class FireOnDropSystem : EntitySystem
{
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly IRobustRandom _random = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GunComponent, LandEvent>(HandleLand);
    }


    private void HandleLand(EntityUid uid, GunComponent component, ref LandEvent args)
    {
        if (_random.Prob(component.FireOnDropChance))
            // The gun fires itself (weird), with the target being its own position offset by its rotation as a point vector.
            // The result being that it will always fire the direction that all gun sprites point in.
            _gun.AttemptShoot(uid, uid, component, Transform(uid).Coordinates.Offset(Transform(uid).LocalRotation.ToVec()));
    }
}
