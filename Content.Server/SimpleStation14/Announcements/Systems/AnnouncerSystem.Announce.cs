using System.Linq;
using Content.Shared.SimpleStation14.Announcements.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Player;

namespace Content.Server.SimpleStation14.Announcements.Systems;

public sealed partial class AnnouncerSystem
{
    /// <summary>
    ///     Gets an announcement path from the announcer
    /// </summary>
    /// <param name="announcementId">ID of the announcement from the announcer to get information from</param>
    /// <param name="announcerId">ID of the announcer to use instead of the current one</param>
    private string GetAnnouncementPath(string announcementId, string? announcerId = null)
    {
        var announcer = announcerId != null ? _prototypeManager.Index<AnnouncerPrototype>(announcerId) : Announcer;

        // Get the announcement data from the announcer
        // Will be the fallback if the data for the announcementId is not found
        var announcementType = announcer.AnnouncementPaths.FirstOrDefault(a => a.ID == announcementId) ??
            announcer.AnnouncementPaths.First(a => a.ID.ToLower() == "fallback");

        // If the greedy announcementType wants to do the job of announcer, ignore the base path and just return the path
        if (announcementType.IgnoreBasePath)
            return announcementType.Path!;
        // If the announcementType has a collection, get the sound from the collection
        if (announcementType.Collection != null)
            return _audioSystem.GetSound(new SoundCollectionSpecifier(announcementType.Collection));
        // If nothing is overriding the base paths, return the base path + the announcement file path
        return $"{announcer.BasePath}/{announcementType.Path}";
    }

    /// <summary>
    ///     Gets audio params from the announcer
    /// </summary>
    /// <param name="announcementId">ID of the announcement from the announcer to get information from</param>
    /// <param name="announcerId">ID of the announcer to use instead of the current one</param>
    private AudioParams? GetAudioParams(string announcementId, string? announcerId = null)
    {
        // Fetch the announcer prototype if a different one is specified
        var announcer = announcerId != null && announcerId != Announcer.ID ? _prototypeManager.Index<AnnouncerPrototype>(announcerId) : Announcer;

        // Get the announcement data from the announcer
        // Will be the fallback if the data for the announcementId is not found
        var announcementType = announcer.AnnouncementPaths.FirstOrDefault(a => a.ID == announcementId) ??
            announcer.AnnouncementPaths.First(a => a.ID == "fallback");

        // Return the announcer.BaseAudioParams if the announcementType doesn't have an override
        return announcementType.AudioParams ?? announcer.BaseAudioParams ?? null; // For some reason the formatter doesn't warn me about "?? null" being redundant
    }

    /// <summary>
    ///     Gets an announcement message from the announcer
    /// </summary>
    /// <param name="announcementId">ID of the announcement from the announcer to get information from</param>
    private string? GetAnnouncementMessage(string announcementId)
    {
        // Get the announcement data from the announcer
        // Will be the fallback if the data for the announcementId is not found
        var announcementType = Announcer.AnnouncementPaths.FirstOrDefault(a => a.ID == announcementId) ??
            Announcer.AnnouncementPaths.First(a => a.ID == "fallback");

        // Return the announcementType.MessageOverride if it exists, otherwise return null
        return announcementType.MessageOverride != null ? Loc.GetString(announcementType.MessageOverride) : null;
    }


    /// <summary>
    ///     Sends an announcement audio
    /// </summary>
    /// <param name="announcementId">ID of the announcement to get information from</param>
    /// <param name="filter">Who hears the announcement audio</param>
    public void SendAnnouncementAudio(string announcementId, Filter filter)
    {
        var announcement = GetAnnouncementPath(announcementId.ToLower());
        _audioSystem.PlayGlobal(announcement, filter, true, GetAudioParams(announcementId.ToLower()));
    }

    /// <summary>
    ///     Sends an announcement message
    /// </summary>
    /// <param name="announcementId">ID of the announcement to get information from</param>
    /// <param name="message">Text to send in the announcement</param>
    /// <param name="sender">Who to show as the announcement announcer, defaults to the current announcer's name</param>
    /// <param name="colorOverride">What color the announcement should be</param>
    /// <param name="station">Station ID to send the announcement to</param>
    public void SendAnnouncementMessage(string announcementId, string message, string? sender = null, Color? colorOverride = null, EntityUid? station = null)
    {
        sender ??= Announcer.Name;

        // If the announcement has a message override, use that instead of the message parameter
        if (GetAnnouncementMessage(announcementId) is { } announcementMessage)
            message = announcementMessage;

        // If there is a station, send the announcement to the station, otherwise send it to everyone
        if (station == null)
            _chatSystem.DispatchGlobalAnnouncement(message, sender, false, colorOverride: colorOverride);
        else
            _chatSystem.DispatchStationAnnouncement(station.Value, message, sender, false,
                colorOverride: colorOverride);
    }

    /// <summary>
    ///     Sends an announcement with a message and audio
    /// </summary>
    /// <param name="announcementId">ID of the announcement to get information from</param>
    /// <param name="filter">Who hears the announcement audio</param>
    /// <param name="message">Text to send in the announcement</param>
    /// <param name="sender">Who to show as the announcement announcer, defaults to the current announcer's name</param>
    /// <param name="colorOverride">What color the announcement should be</param>
    /// <param name="station">Station ID to send the announcement to</param>
    public void SendAnnouncement(string announcementId, Filter filter, string message, string? sender = null, Color? colorOverride = null, EntityUid? station = null)
    {
        SendAnnouncementAudio(announcementId, filter);
        SendAnnouncementMessage(announcementId, message, sender, colorOverride, station);
    }
}
