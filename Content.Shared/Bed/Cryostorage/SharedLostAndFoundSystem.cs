using Content.Shared.Administration.Logs;

namespace Content.Shared.Bed.Cryostorage;

/// <summary>
/// This handles <see cref="LostAndFoundComponent"/>
/// </summary>
public abstract class SharedLostAndFoundSystem : EntitySystem
{
    [Dependency] protected readonly ISharedAdminLogManager AdminLog = default!;

    protected EntityUid? PausedMap { get; private set; }

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<LostAndFoundComponent, ComponentShutdown>(OnShutdownContainer);
    }

    private void OnShutdownContainer(Entity<LostAndFoundComponent> ent, ref ComponentShutdown args)
    {
        var comp = ent.Comp;
        foreach (var stored in comp.StoredPlayers)
        {
            if (TryComp<CryostorageContainedComponent>(stored, out var containedComponent))
            {
                containedComponent.Cryostorage = null;
                Dirty(stored, containedComponent);
            }
        }

        comp.StoredPlayers.Clear();
        Dirty(ent, comp);
    }

    public bool IsInPausedMap(Entity<TransformComponent?> entity)
    {
        var (_, comp) = entity;
        comp ??= Transform(entity);

        return comp.MapUid != null && comp.MapUid == PausedMap;
    }
}
