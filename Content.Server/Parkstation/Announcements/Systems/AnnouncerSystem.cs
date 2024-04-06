using Content.Server.Chat.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Parkstation.Announcements.Prototypes;
using Content.Shared.Parkstation.Announcements.Systems;
using Content.Shared.Parkstation.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Parkstation.Announcements.Systems;

public sealed partial class AnnouncerSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedAnnouncerSystem _announcer = default!;

    /// <summary>
    ///     The currently selected announcer
    /// </summary>
    [Access(typeof(AnnouncerSystem))]
    public AnnouncerPrototype Announcer { get; set; } = default!;


    public override void Initialize()
    {
        base.Initialize();

        PickAnnouncer();

        _config.OnValueChanged(SimpleStationCCVars.Announcer, SetAnnouncer);

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestarting);
    }
}
