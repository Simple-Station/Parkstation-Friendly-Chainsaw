using System.Globalization;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.Station.Systems;
using Content.Shared.Administration;
using Content.Shared.Roles;
using Robust.Shared.Audio;
using Robust.Shared.Console;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Content.Shared.Inventory;
using Content.Server.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Mind;
using Content.Shared.Roles.Jobs;

namespace Content.Server.Administration.Commands.Cryostasis
{
    [AnyCommand]
    public sealed class CryostasisCommand : IConsoleCommand
    {
        [Dependency] private readonly IEntityManager _entities = default!;
        [Dependency] private static readonly IEntitySystemManager _entitysys = default!;
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;

        private readonly SharedRoleSystem _roleSystem = _entitysys.GetEntitySystem<SharedRoleSystem>();

        public string Command => "cryostasis";
        public string Description => "Deletes you and opens up a new job slot. Do this in a secure area or put your belongings in a secure area. MISUSE WILL BE MODERATED";
        public string Help => $"Usage: {Command}";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var player = shell.Player;
            // Check if not a player
            if (player == null)
            {
                shell.WriteLine("You aren't a player.");
                return;
            }

            // Check if player has a mind
            var minds = _entities.System<SharedMindSystem>();
            if (!minds.TryGetMind(player, out var mindId, out var mind))
            {
                shell.WriteLine("You can't do this without a mind.");
                return;
            }

            if (mind.IsVisitingEntity)
            {
                shell.WriteLine("You cannot do this while visiting something.");
                return;
            }

            // Check if player has a job (unemployed people are not allowed to go into cryogenics).
            if (!_roleSystem.MindHasRole<JobComponent>(mindId))
            {
                shell.WriteLine("You do not have a job, you are not accessible by Nanotrasen, therefore unable to cryo.");
                return;
            }

            // No entity somehow???
            if (mind.CurrentEntity == null)
            {
                shell.WriteLine("You do not have an entity?");
                return;
            }

            // Various definitions
            var uid = (EntityUid) mind.CurrentEntity;
            RoleInfo? job = null;
            JobPrototype? jobprotot = null;
            var _prototypeManager = IoCManager.Resolve<IPrototypeManager>();
            var _stationJobs = EntitySystem.Get<StationJobsSystem>();
            var isantag = false;

            // Check all roles
            foreach (var role in _roleSystem.MindGetAllRoles(mindId))
            {
                // Can't do this if antag, set a variable for later use.
                if (role.Antagonist == true)
                {
                    isantag = true;
                    continue;
                }
                // A job has been found, stop looking.
                if (job != null) continue;

                // A job has been found, remove it from the mind (you are passing this job onto a latejoiner).
                job = role;
                _roleSystem.MindRemoveRole<JobComponent>(mindId);
            }

            // No job, you aren't a Nanotrasen employee. Probably some off-station role or random ghost(role).
            if (job == null)
            {
                shell.WriteLine("You do not have a job, you are not accessible by Nanotrasen, therefore unable to cryo.");
                return;
            }

            // Disappearing randomly as an antag wouldn't be very fun. An admin can handle it if really needed.
            if (isantag == true)
            {
                shell.WriteLine("You are an antagonist, the Syndicate would not like this, and Nanotrasen will get too easy of a resolution. This would be inappropriate.");
                return;
            }

            // Find a job prototype matching the mind role name (reverse Loc wooo!).
            foreach (var jobproto in _prototypeManager.EnumeratePrototypes(typeof(JobPrototype)))
            {
                if (typeof(JobPrototype) != typeof(JobPrototype)) continue;

                var jobprotoo = (JobPrototype) jobproto;
                var jobb = (RoleInfo) job;
                if (Loc.GetString(jobprotoo.Name).ToLower() == jobb.Name.ToLower())
                    jobprotot = jobprotoo;
            }

            // No matching job, Nanotrasen didn't employ you, who hired you? (jobs should ONLY be made for Nanotrasen ships.)
            if (jobprotot == null)
            {
                shell.WriteLine("You do not have a job, you are not accessible by Nanotrasen, therefore unable to cryo.");
                return;
            }

            // Adminlogs
            _adminLogger.Add(LogType.Mind, LogImpact.High, $"{_entities.ToPrettyString(uid):player} is going into cryostasis");

            // TODO: put items in a box? a pile of items is *fine* but a box of sorts might be nice.
            // Unequip all their items, in case they didn't do what was advised in the description or someone needs something from them.
            _entities.TryGetComponent<InventoryComponent>(uid, out var inventoryComponent);
            var invSystem = _entities.System<InventorySystem>();
            if (invSystem.TryGetSlots(uid, out var slotDefinitions, inventoryComponent))
            {
                foreach (var slot in slotDefinitions)
                {
                    invSystem.TryUnequip(uid, slot.Name, true, true, false, inventoryComponent);
                }
            }

            // Ghost them, if they can't be, tell them.
            if (!EntitySystem.Get<GameTicker>().OnGhostAttempt(mindId, false))
            {
                shell.WriteLine("You can't ghost right now.");
                return;
            }

            // Play a sound.
            SoundSystem.Play("/Audio/SimpleStation14/Effects/cryostasis.ogg", Filter.Pvs(uid), uid);

            // TODO: check that this works when multiple stations ever get loaded regularly at the same time, both with jobs.
            // Find the first station (very likely the one with the jobs)
            EntityUid? station = null;
            station = EntitySystem.Get<StationSystem>().GetOwningStation(mindId);

            // Send a cryostasis announcement to the station, if any.
            if (station != null)
            {
                var statio = (EntityUid) station;

                EntitySystem.Get<ChatSystem>().DispatchStationAnnouncement(statio,
                    Loc.GetString("cryo-departure-announcement",
                        ("character", _entities.GetComponent<MetaDataComponent>(uid).EntityName),
                        ("job", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Loc.GetString(jobprotot.Name)))),
                    Loc.GetString("latejoin-arrival-sender"),
                    playDefaultSound: false);

                if (!_stationJobs.IsJobUnlimited(statio, jobprotot))
                {
                    _stationJobs.TryGetJobSlot(statio, jobprotot, out var slots);
                    if (slots != null)
                    {
                        _stationJobs.TryAdjustJobSlot(statio, jobprotot, (int) (slots + 1));
                    }
                }
            }
            else
                shell.WriteLine("No station found, leave announcement will not be sent.");


            // Put them into "Cryostasis".
            _entities.QueueDeleteEntity(uid);
        }
    }
}
