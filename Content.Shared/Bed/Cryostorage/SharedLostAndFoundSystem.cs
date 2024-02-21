using Content.Shared.Administration.Logs;
using Content.Shared.GameTicking;
using Robust.Shared.Containers;
using Robust.Shared.Map;

namespace Content.Shared.Bed.Cryostorage;

/// <summary>
/// This handles <see cref="LostAndFoundComponent"/>
/// </summary>
public abstract class SharedLostAndFoundSystem : EntitySystem
{
    [Dependency] protected readonly ISharedAdminLogManager AdminLog = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;

    public EntityUid? PausedMap { get; private set; }

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<LostAndFoundComponent, ComponentShutdown>(OnShutdownContainer);
        SubscribeLocalEvent<LostAndFoundComponent, EntGotRemovedFromContainerMessage>(OnRemovedContained);

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
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

    private void OnRemovedContained(Entity<LostAndFoundComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        var (uid, comp) = ent;
        if (!IsInPausedMap(uid))
            RemCompDeferred(ent, comp);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent _)
    {
        DeletePausedMap();
    }

    private void DeletePausedMap()
    {
        if (PausedMap == null || !Exists(PausedMap))
            return;

        EntityManager.DeleteEntity(PausedMap.Value);
        PausedMap = null;
    }

    public void EnsurePausedMap()
    {
        if (PausedMap != null && Exists(PausedMap))
            return;

        var map = _mapManager.CreateMap();
        _mapManager.SetMapPaused(map, true);
        PausedMap = _mapManager.GetMapEntityId(map);
    }
}
