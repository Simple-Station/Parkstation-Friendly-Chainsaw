using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.Bed.Cryostorage;
using Content.Shared.Chat;
using Content.Shared.Climbing.Systems;
using Content.Shared.Database;
using Content.Shared.Mind.Components;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Enums;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server.Bed.Cryostorage;

/// <inheritdoc/>
public sealed class CryostorageSystem : SharedCryostorageSystem
{
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ClimbSystem _climb = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly StationJobsSystem _stationJobs = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly LostAndFoundSystem _lostAndFound = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CryostorageContainedComponent, PlayerSpawnCompleteEvent>(OnPlayerSpawned);
        SubscribeLocalEvent<CryostorageContainedComponent, MindRemovedMessage>(OnMindRemoved);

        _playerManager.PlayerStatusChanged += PlayerStatusChanged;
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _playerManager.PlayerStatusChanged -= PlayerStatusChanged;
    }

    private void OnPlayerSpawned(Entity<CryostorageContainedComponent> ent, ref PlayerSpawnCompleteEvent args)
    {
        // if you spawned into cryostorage, we're not gonna round-remove you.
        ent.Comp.GracePeriodEndTime = null;
    }

    private void OnMindRemoved(Entity<CryostorageContainedComponent> ent, ref MindRemovedMessage args)
    {
        var comp = ent.Comp;

        if (!TryComp<CryostorageComponent>(comp.Cryostorage, out var cryostorageComponent))
            return;

        if (comp.GracePeriodEndTime != null)
            comp.GracePeriodEndTime = Timing.CurTime + cryostorageComponent.NoMindGracePeriod;
        comp.AllowReEnteringBody = false;
        comp.UserId = args.Mind.Comp.UserId;
    }

    private void PlayerStatusChanged(object? sender, SessionStatusEventArgs args)
    {
        if (args.Session.AttachedEntity is not { } entity)
            return;

        if (!TryComp<CryostorageContainedComponent>(entity, out var containedComponent))
            return;

        if (args.NewStatus is SessionStatus.Disconnected or SessionStatus.Zombie)
        {
            containedComponent.AllowReEnteringBody = true;
            var delay = CompOrNull<CryostorageComponent>(containedComponent.Cryostorage)?.NoMindGracePeriod ?? TimeSpan.Zero;
            containedComponent.GracePeriodEndTime = Timing.CurTime + delay;
            containedComponent.UserId = args.Session.UserId;
        }
        else if (args.NewStatus == SessionStatus.InGame)
        {
            HandleCryostorageReconnection((entity, containedComponent));
        }
    }

    public void HandleEnterCryostorage(Entity<CryostorageContainedComponent> ent, NetUserId? userId)
    {
        var comp = ent.Comp;
        var cryostorageEnt = ent.Comp.Cryostorage;
        if (!TryComp<CryostorageComponent>(cryostorageEnt, out var cryostorageComponent))
            return;

        // if we have a session, we use that to add back in all the job slots the player had.
        if (userId != null)
        {
            foreach (var station in _station.GetStationsSet())
            {
                if (!TryComp<StationJobsComponent>(station, out var stationJobs))
                    continue;

                if (!_stationJobs.TryGetPlayerJobs(station, userId.Value, out var jobs, stationJobs))
                    continue;

                foreach (var job in jobs)
                {
                    _stationJobs.TryAdjustJobSlot(station, job, 1, clamp: true);
                }

                _stationJobs.TryRemovePlayerJobs(station, userId.Value, stationJobs);
            }
        }

        // play the cryostasis sound effect; need to use coordinates since the body gets deleted
        _audio.PlayPvs("/Audio/SimpleStation14/Effects/cryostasis.ogg", Transform(ent).Coordinates, AudioParams.Default.WithVolume(6f));

        _lostAndFound.EnsurePausedMap();
        if (_lostAndFound.PausedMap == null)
        {
            Log.Error("CryoSleep map was unexpectedly null");
            return;
        }

        if (!CryoSleepRejoiningEnabled || !comp.AllowReEnteringBody)
        {
            if (userId != null && Mind.TryGetMind(userId.Value, out var mind))
            {
                _gameTicker.OnGhostAttempt(mind.Value, false);
            }
        }
        comp.AllowReEnteringBody = false;
        _transform.SetParent(ent, _lostAndFound.PausedMap.Value);

        // try to get the lost and found and add the player to it
        var query = EntityQueryEnumerator<LostAndFoundComponent>();
        query.MoveNext(out var storage, out var lostAndFoundComponent);

        if (TryComp<LostAndFoundComponent>(storage, out var lostAndFoundComp))
        {
            lostAndFoundComp.StoredPlayers.Add(ent);
            Dirty(ent, comp);
            _lostAndFound.UpdateCryostorageUIState((storage, lostAndFoundComp));

        }

        AdminLog.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(ent):player} was entered into cryostorage inside of {ToPrettyString(cryostorageEnt.Value)}");
    }

    private void HandleCryostorageReconnection(Entity<CryostorageContainedComponent> entity)
    {
        var (uid, comp) = entity;
        if (!CryoSleepRejoiningEnabled || !_lostAndFound.IsInPausedMap(uid))
            return;

        // how did you destroy these? they're indestructible.
        if (comp.Cryostorage is not { } cryostorage ||
            TerminatingOrDeleted(cryostorage) ||
            !TryComp<CryostorageComponent>(cryostorage, out var cryostorageComponent))
        {
            QueueDel(entity);
            return;
        }

        var cryoXform = Transform(cryostorage);
        _transform.SetParent(uid, cryoXform.ParentUid);
        _transform.SetCoordinates(uid, cryoXform.Coordinates);
        if (!_container.TryGetContainer(cryostorage, cryostorageComponent.ContainerId, out var container) ||
            !_container.Insert(uid, container, cryoXform))
        {
            _climb.ForciblySetClimbing(uid, cryostorage);
        }

        comp.GracePeriodEndTime = null;

        // try to get the lost and found and remove the player from it
        var query = EntityQueryEnumerator<LostAndFoundComponent>();
        query.MoveNext(out var storage, out var lostAndFoundComponent);

        if (TryComp<LostAndFoundComponent>(storage, out var lostAndFoundComp))
        {
            lostAndFoundComp.StoredPlayers.Remove(uid);
            _lostAndFound.UpdateCryostorageUIState((storage, lostAndFoundComp));
        }

        AdminLog.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(entity):player} re-entered the game from cryostorage {ToPrettyString(cryostorage)}");
    }

    protected override void OnInsertedContainer(Entity<CryostorageComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        var (uid, comp) = ent;
        if (args.Container.ID != comp.ContainerId)
            return;

        base.OnInsertedContainer(ent, ref args);

        var locKey = CryoSleepRejoiningEnabled
            ? "cryostorage-insert-message-temp"
            : "cryostorage-insert-message-permanent";

        var msg = Loc.GetString(locKey, ("time", comp.GracePeriod.TotalMinutes));
        if (TryComp<ActorComponent>(args.Entity, out var actor))
            _chatManager.ChatMessageToOne(ChatChannel.Server, msg, msg, uid, false, actor.PlayerSession.Channel);
    }


    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CryostorageContainedComponent>();
        while (query.MoveNext(out var uid, out var containedComp))
        {
            if (containedComp.GracePeriodEndTime == null)
                continue;

            if (Timing.CurTime < containedComp.GracePeriodEndTime)
                continue;

            Mind.TryGetMind(uid, out _, out var mindComp);
            var id = mindComp?.UserId ?? containedComp.UserId;
            HandleEnterCryostorage((uid, containedComp), id);
        }
    }
}
