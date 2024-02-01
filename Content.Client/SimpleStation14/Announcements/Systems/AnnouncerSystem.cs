using Content.Shared.SimpleStation14.Announcements.Events;
using Content.Shared.SimpleStation14.Announcements.Systems;
using Content.Shared.SimpleStation14.CCVar;
using Robust.Client.Audio;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Shared.Audio.Sources;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;

namespace Content.Client.SimpleStation14.Announcements.Systems;

public sealed class AnnouncerSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IResourceCache _cache = default!;
    [Dependency] private readonly IAudioManager _audioManager = default!;
    [Dependency] private readonly SharedAnnouncerSystem _announcer = default!;

    private IAudioSource? AnnouncerSource { get; set; }
    private float AnnouncerVolume { get; set; }


    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<AnnouncementSendEvent>(OnAnnouncementReceived);
        _config.OnValueChanged(SimpleStationCCVars.AnnouncerVolume, OnAnnouncerVolumeChanged);
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _config.UnsubValueChanged(SimpleStationCCVars.AnnouncerVolume, OnAnnouncerVolumeChanged);
    }


    private void OnAnnouncerVolumeChanged(float value)
    {
        AnnouncerVolume = value;

        if (AnnouncerSource != null)
            AnnouncerSource.Gain = AnnouncerVolume;
    }

    private void OnAnnouncementReceived(AnnouncementSendEvent ev)
    {
        if (!ev.Recipients.Contains(_player.LocalSession!.UserId))
            return;

        var resource = _cache.GetResource<AudioResource>(_announcer.GetAnnouncementPath(ev.AnnouncementId, ev.AnnouncerId)!);
        var source = _audioManager.CreateAudioSource(resource);

        if (source != null)
        {
            source.Gain = AnnouncerVolume * SharedAudioSystem.VolumeToGain(ev.AudioParams.Volume);
            source.Global = true;
        }

        AnnouncerSource?.Dispose();
        AnnouncerSource = source;
        AnnouncerSource?.StartPlaying();
    }
}
