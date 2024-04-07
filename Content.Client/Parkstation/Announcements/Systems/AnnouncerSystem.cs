using Content.Client.Audio;
using Content.Shared.Parkstation.Announcements.Events;
using Content.Shared.Parkstation.Announcements.Systems;
using Content.Shared.Parkstation.CCVar;
using Robust.Client.Audio;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Shared.Audio.Sources;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;

namespace Content.Client.Parkstation.Announcements.Systems;

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

        AnnouncerVolume = _config.GetCVar(SimpleStationCCVars.AnnouncerVolume) * 100f / ContentAudioSystem.AnnouncerMultiplier;

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
        if (!ev.Recipients.Contains(_player.LocalSession!.UserId)
            || !_cache.TryGetResource<AudioResource>(_announcer.GetAnnouncementPath(ev.AnnouncementId, ev.AnnouncerId),
                out var resource))
            return;

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
