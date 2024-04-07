using System.Diagnostics.CodeAnalysis;
using Content.Server.Administration.Logs;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Parkstation.Announcements.Systems;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Server.StationEvents.Components;
using Content.Shared.Database;
using Content.Shared.Parkstation.Announcements.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Collections;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.StationEvents.Events;

/// <summary>
///     An abstract entity system inherited by all station events for their behavior.
/// </summary>
public abstract partial class StationEventSystem<T> : GameRuleSystem<T> where T : IComponent
{
    [Dependency] protected readonly IAdminLogManager AdminLogManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] protected readonly IMapManager MapManager = default!;
    [Dependency] protected readonly IPrototypeManager PrototypeManager = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;
    [Dependency] protected readonly ChatSystem ChatSystem = default!;
    [Dependency] protected readonly SharedAudioSystem Audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] protected readonly StationSystem StationSystem = default!;
    [Dependency] private readonly AnnouncerSystem _announcer = default!;

    protected ISawmill Sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();

        Sawmill = Logger.GetSawmill("stationevents");
    }

    /// <inheritdoc/>
    protected override void Added(EntityUid uid, T component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, component, gameRule, args);

        if (!TryComp<StationEventComponent>(uid, out var stationEvent))
            return;

        AdminLogManager.Add(LogType.EventAnnounced, $"Event added / announced: {ToPrettyString(uid)}");

        stationEvent.StartTime = _timing.CurTime + stationEvent.StartDelay;
    }

    /// <inheritdoc/>
    protected override void Started(EntityUid uid, T component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (!TryComp<StationEventComponent>(uid, out var stationEvent))
            return;

        AdminLogManager.Add(LogType.EventStarted, LogImpact.High, $"Event started: {ToPrettyString(uid)}");

        // Parkstation-RandomAnnouncers-Start
        if (stationEvent.StartAnnouncement)
        {
            _announcer.SendAnnouncement(_announcer.GetAnnouncementId(args.RuleId), Filter.Broadcast(),
                Loc.GetString(_announcer.GetEventLocaleString(_announcer.GetAnnouncementId(args.RuleId))),
                colorOverride: Color.Gold);
        }
        // Parkstation-RandomAnnouncers-End

        if (stationEvent.Duration != null)
        {
            var duration = stationEvent.MaxDuration == null
                ? stationEvent.Duration
                : TimeSpan.FromSeconds(RobustRandom.NextDouble(stationEvent.Duration.Value.TotalSeconds,
                    stationEvent.MaxDuration.Value.TotalSeconds));
            stationEvent.EndTime = _timing.CurTime + duration;
        }
    }

    /// <inheritdoc/>
    protected override void Ended(EntityUid uid, T component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);

        if (!TryComp<StationEventComponent>(uid, out var stationEvent))
            return;

        AdminLogManager.Add(LogType.EventStopped, $"Event ended: {ToPrettyString(uid)}");

        // Parkstation-RandomAnnouncers-Start
        if (stationEvent.EndAnnouncement)
        {
            _announcer.SendAnnouncement(_announcer.GetAnnouncementId(args.RuleId, true), Filter.Broadcast(),
                Loc.GetString(_announcer.GetEventLocaleString(_announcer.GetAnnouncementId(args.RuleId, true))),
                colorOverride: Color.Gold);
        }
        // Parkstation-RandomAnnouncers-End
    }

    /// <summary>
    ///     Called every tick when this event is running.
    ///     Events are responsible for their own lifetime, so this handles starting and ending after time.
    /// </summary>
    /// <inheritdoc/>
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<StationEventComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var stationEvent, out var ruleData))
        {
            if (!GameTicker.IsGameRuleAdded(uid, ruleData))
                continue;

            if (!GameTicker.IsGameRuleActive(uid, ruleData) && _timing.CurTime >= stationEvent.StartTime)
            {
                GameTicker.StartGameRule(uid, ruleData);
            }
            else if (stationEvent.EndTime != null && _timing.CurTime >= stationEvent.EndTime && GameTicker.IsGameRuleActive(uid, ruleData))
            {
                GameTicker.EndGameRule(uid, ruleData);
            }
        }
    }

    #region Helper Functions

    protected void ForceEndSelf(EntityUid uid, GameRuleComponent? component = null)
    {
        GameTicker.EndGameRule(uid, component);
    }

    public float GetSeverityModifier()
    {
        var ev = new GetSeverityModifierEvent();
        RaiseLocalEvent(ev);
        return ev.Modifier;
    }

    #endregion
}

/// <summary>
///     Raised broadcast to determine what the severity modifier should be for an event, some positive number that can be multiplied with various things.
///     Handled by usually other game rules (like the ramping scheduler).
///     Most events should try and make use of this if possible.
/// </summary>
public sealed class GetSeverityModifierEvent : EntityEventArgs
{
    /// <summary>
    ///     Should be multiplied/added to rather than set, for commutativity.
    /// </summary>
    public float Modifier = 1.0f;
}
