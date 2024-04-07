﻿using Content.Server.GameTicking.Rules.Components;
using Content.Server.Parkstation.Announcements.Systems;
using Content.Server.StationEvents.Components;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server.StationEvents.Events;

public sealed class BluespaceArtifactRule : StationEventSystem<BluespaceArtifactRuleComponent>
{
    [Dependency] private readonly AnnouncerSystem _announcer = default!;

    protected override void Added(EntityUid uid, BluespaceArtifactRuleComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, component, gameRule, args);

        var str = Loc.GetString("bluespace-artifact-event-announcement",
            ("sighting", Loc.GetString(RobustRandom.Pick(component.PossibleSighting))));

        // Parkstation-RandomAnnouncers-Start
        _announcer.SendAnnouncement(_announcer.GetAnnouncementId(args.RuleId), Filter.Broadcast(), str,
            colorOverride: Color.FromHex("#18abf5"));
        // Parkstation-RandomAnnouncers-End
    }

    protected override void Started(EntityUid uid, BluespaceArtifactRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var amountToSpawn = Math.Max(1, (int) MathF.Round(GetSeverityModifier() / 1.5f));
        for (var i = 0; i < amountToSpawn; i++)
        {
            if (!TryFindRandomTile(out _, out _, out _, out var coords))
                return;

            Spawn(component.ArtifactSpawnerPrototype, coords);
            Spawn(component.ArtifactFlashPrototype, coords);

            Sawmill.Info($"Spawning random artifact at {coords}");
        }
    }
}
