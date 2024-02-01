using System.Linq;
using Content.Shared.GameTicking;
using Content.Shared.SimpleStation14.Announcements.Prototypes;
using Content.Shared.SimpleStation14.CCVar;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server.SimpleStation14.Announcements.Systems;

public sealed partial class AnnouncerSystem
{
    private void OnRoundRestarting(RoundRestartCleanupEvent ev)
    {
        var announcer = _config.GetCVar(SimpleStationCCVars.Announcer);
        if (string.IsNullOrEmpty(announcer))
            SetAnnouncer(PickAnnouncer());
        else
            SetAnnouncer(announcer);
    }


    /// <summary>
    ///     Picks a random announcer
    /// </summary>
    /// <remarks>Probably not very useful for any other system</remarks>
    public AnnouncerPrototype PickAnnouncer()
    {
        return _random.Pick(_proto.EnumeratePrototypes<AnnouncerPrototype>()
            .Where(x => !_config.GetCVar(SimpleStationCCVars.AnnouncerBlacklist).Contains(x.ID))
            .ToArray());
    }


    /// <summary>
    ///     Sets the announcer
    /// </summary>
    /// <param name="announcerId">ID of the announcer to choose</param>
    public void SetAnnouncer(string announcerId)
    {
        if (!_proto.TryIndex<AnnouncerPrototype>(announcerId, out var announcer))
            DebugTools.Assert("Set announcer does not exist, attempting to use previously set one.");
        else
            Announcer = announcer;
    }

    /// <summary>
    ///     Sets the announcer
    /// </summary>
    /// <param name="announcer">The announcer prototype to set the current announcer to</param>
    public void SetAnnouncer(AnnouncerPrototype announcer)
    {
        Announcer = announcer;
    }
}
