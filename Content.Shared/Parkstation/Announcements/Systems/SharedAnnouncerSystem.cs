using System.Linq;
using Content.Shared.Parkstation.Announcements.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared.Parkstation.Announcements.Systems;

public sealed class SharedAnnouncerSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;


    /// <summary>
    ///     Gets an announcement path from the announcer
    /// </summary>
    /// <param name="announcementId">ID of the announcement from the announcer to get information for</param>
    /// <param name="announcerId">ID of the announcer to use instead of the current one</param>
    public string GetAnnouncementPath(string announcementId, string announcerId)
    {
        if (!_proto.TryIndex<AnnouncerPrototype>(announcerId, out var announcer))
            return "";

        // Get the announcement data from the announcer
        // Will be the fallback if the data for the announcementId is not found
        var announcementType = announcer.Announcements.FirstOrDefault(a => a.ID == announcementId) ??
            announcer.Announcements.First(a => a.ID.ToLower() == "fallback");

        // If the greedy announcementType wants to do the job of announcer, ignore the base path and just return the path
        if (announcementType.IgnoreBasePath)
            return announcementType.Path!;
        // If the announcementType has a collection, get the sound from the collection
        if (announcementType.Collection != null)
            return _audio.GetSound(new SoundCollectionSpecifier(announcementType.Collection));
        // If nothing is overriding the base paths, return the base path + the announcement file path
        return $"{announcer.BasePath}/{announcementType.Path}";
    }

    /// <summary>
    ///     Gets audio params from the announcer
    /// </summary>
    /// <param name="announcementId">ID of the announcement from the announcer to get information for</param>
    /// <param name="announcer">Announcer prototype to get information from</param>
    public string GetAnnouncementPath(string announcementId, AnnouncerPrototype announcer)
    {
        // Get the announcement data from the announcer
        // Will be the fallback if the data for the announcementId is not found
        var announcementType = announcer.Announcements.FirstOrDefault(a => a.ID == announcementId) ??
            announcer.Announcements.First(a => a.ID.ToLower() == "fallback");

        // If the greedy announcementType wants to do the job of announcer, ignore the base path and just return the path
        if (announcementType.IgnoreBasePath)
            return announcementType.Path!;
        // If the announcementType has a collection, get the sound from the collection
        if (announcementType.Collection != null)
            return _audio.GetSound(new SoundCollectionSpecifier(announcementType.Collection));
        // If nothing is overriding the base paths, return the base path + the announcement file path
        return $"{announcer.BasePath}/{announcementType.Path}";
    }


    /// <summary>
    ///     Gets audio params from the announcer
    /// </summary>
    /// <param name="announcementId">ID of the announcement from the announcer to get information from</param>
    /// <param name="announcerId">ID of the announcer to use instead of the current one</param>
    public AudioParams? GetAudioParams(string announcementId, string announcerId)
    {
        if (!_proto.TryIndex<AnnouncerPrototype>(announcerId, out var announcer))
            return null;

        // Get the announcement data from the announcer
        // Will be the fallback if the data for the announcementId is not found
        var announcementType = announcer.Announcements.FirstOrDefault(a => a.ID == announcementId) ??
            announcer.Announcements.First(a => a.ID == "fallback");

        // Return the announcer.BaseAudioParams if the announcementType doesn't have an override
        return announcementType.AudioParams ?? announcer.BaseAudioParams ?? null; // For some reason the formatter doesn't warn me about "?? null" being redundant, so it stays
    }

    /// <summary>
    ///     Gets audio params from the announcer
    /// </summary>
    /// <param name="announcementId">ID of the announcement from the announcer to get information from</param>
    /// <param name="announcer">Announcer prototype to get information from</param>
    public AudioParams? GetAudioParams(string announcementId, AnnouncerPrototype announcer)
    {
        // Get the announcement data from the announcer
        // Will be the fallback if the data for the announcementId is not found
        var announcementType = announcer.Announcements.FirstOrDefault(a => a.ID == announcementId) ??
            announcer.Announcements.First(a => a.ID == "fallback");

        // Return the announcer.BaseAudioParams if the announcementType doesn't have an override
        return announcementType.AudioParams ?? announcer.BaseAudioParams;
    }


    /// <summary>
    ///     Gets an announcement message from the announcer
    /// </summary>
    /// <param name="announcementId">ID of the announcement from the announcer to get information from</param>
    /// <param name="announcerId">ID of the announcer to get information from</param>
    public string? GetAnnouncementMessage(string announcementId, string announcerId)
    {
        if (!_proto.TryIndex<AnnouncerPrototype>(announcerId, out var announcer))
            return null;

        // Get the announcement data from the announcer
        // Will be the fallback if the data for the announcementId is not found
        var announcementType = announcer.Announcements.FirstOrDefault(a => a.ID == announcementId) ??
                               announcer.Announcements.First(a => a.ID == "fallback");

        // Return the announcementType.MessageOverride if it exists, otherwise return null
        return announcementType.MessageOverride != null ? Loc.GetString(announcementType.MessageOverride) : null;
    }
}
