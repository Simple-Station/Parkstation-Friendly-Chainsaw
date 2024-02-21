using Content.Shared.Administration.Logs;
using Content.Shared.CCVar;
using Content.Shared.DragDrop;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Shared.Bed.Cryostorage;

/// <summary>
/// This handles <see cref="CryostorageComponent"/>
/// </summary>
public abstract class SharedCryostorageSystem : EntitySystem
{
    [Dependency] protected readonly ISharedAdminLogManager AdminLog = default!;
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] protected readonly SharedMindSystem Mind = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    protected bool CryoSleepRejoiningEnabled;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<CryostorageComponent, EntInsertedIntoContainerMessage>(OnInsertedContainer);
        SubscribeLocalEvent<CryostorageComponent, EntRemovedFromContainerMessage>(OnRemovedContainer);
        SubscribeLocalEvent<CryostorageComponent, ContainerIsInsertingAttemptEvent>(OnInsertAttempt);
        SubscribeLocalEvent<CryostorageComponent, CanDropTargetEvent>(OnCanDropTarget);

        SubscribeLocalEvent<CryostorageContainedComponent, EntityUnpausedEvent>(OnUnpaused);
        SubscribeLocalEvent<CryostorageContainedComponent, ComponentShutdown>(OnShutdownContained);


        _configuration.OnValueChanged(CCVars.GameCryoSleepRejoining, OnCvarChanged, true);
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _configuration.UnsubValueChanged(CCVars.GameCryoSleepRejoining, OnCvarChanged);
    }

    private void OnCvarChanged(bool value)
    {
        CryoSleepRejoiningEnabled = value;
    }

    protected virtual void OnInsertedContainer(Entity<CryostorageComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        var (_, comp) = ent;
        if (args.Container.ID != comp.ContainerId)
            return;

        _appearance.SetData(ent, CryostorageVisuals.Full, true);
        if (!Timing.IsFirstTimePredicted)
            return;

        var containedComp = EnsureComp<CryostorageContainedComponent>(args.Entity);
        var delay = Mind.TryGetMind(args.Entity, out _, out _) ? comp.GracePeriod : comp.NoMindGracePeriod;
        containedComp.GracePeriodEndTime = Timing.CurTime + delay;
        containedComp.Cryostorage = ent;
        Dirty(args.Entity, containedComp);

        // play sound, checking for client-side prediction to avoid double audio
        if (!Timing.InPrediction)
            return;

        _audio.PlayPvs("/Audio/SimpleStation14/Effects/cryosleep_open.ogg", ent, AudioParams.Default.WithVolume(6f));
    }

    private void OnRemovedContainer(Entity<CryostorageComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        var (_, comp) = ent;
        if (args.Container.ID != comp.ContainerId)
            return;

        _appearance.SetData(ent, CryostorageVisuals.Full, args.Container.ContainedEntities.Count > 0);
    }

    private void OnInsertAttempt(Entity<CryostorageComponent> ent, ref ContainerIsInsertingAttemptEvent args)
    {
        var (_, comp) = ent;
        if (args.Container.ID != comp.ContainerId)
            return;

        if (!TryComp<MindContainerComponent>(args.EntityUid, out var mindContainer))
        {
            args.Cancel();
            return;
        }

        if (Mind.TryGetMind(args.EntityUid, out _, out var mindComp, mindContainer) &&
            (mindComp.PreventSuicide || mindComp.PreventGhosting))
        {
            args.Cancel();
        }
    }

    private void OnCanDropTarget(Entity<CryostorageComponent> ent, ref CanDropTargetEvent args)
    {
        if (args.Dragged == args.User)
            return;

        if (!Mind.TryGetMind(args.Dragged, out _, out var mindComp) || mindComp.Session?.AttachedEntity != args.Dragged)
            return;

        args.CanDrop = false;
        args.Handled = true;
    }


    private void OnUnpaused(Entity<CryostorageContainedComponent> ent, ref EntityUnpausedEvent args)
    {
        var comp = ent.Comp;
        if (comp.GracePeriodEndTime != null)
            comp.GracePeriodEndTime = comp.GracePeriodEndTime.Value + args.PausedTime;
    }

    private void OnShutdownContained(Entity<CryostorageContainedComponent> ent, ref ComponentShutdown args)
    {
        var comp = ent.Comp;

        // try to get the lost and found and remove the player from it
        var query = EntityQueryEnumerator<LostAndFoundComponent>();
        query.MoveNext(out var storage, out var lostAndFoundComponent);
        CompOrNull<LostAndFoundComponent>(storage)?.StoredPlayers.Remove(ent);

        ent.Comp.Cryostorage = null;
        Dirty(ent, comp);
    }
}
