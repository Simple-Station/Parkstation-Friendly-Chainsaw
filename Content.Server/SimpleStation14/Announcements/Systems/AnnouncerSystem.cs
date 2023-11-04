using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Events;
using Content.Shared.SimpleStation14.Announcements.Prototypes;
using Content.Shared.SimpleStation14.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.SimpleStation14.Announcements.Systems;

public sealed partial class AnnouncerSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _configManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;

    /// <summary>
    ///     The currently selected announcer
    /// </summary>
    public AnnouncerPrototype Announcer { get; set; } = default!;


    public override void Initialize()
    {
        base.Initialize();

        PickAnnouncer();

        _configManager.OnValueChanged(SimpleStationCCVars.Announcer, SetAnnouncer);

        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStarting);
    }


    private void OnRoundStarting(RoundStartingEvent ev)
    {
        SetAnnouncer(_configManager.GetCVar(SimpleStationCCVars.Announcer));
    }
}
