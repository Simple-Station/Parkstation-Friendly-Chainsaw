using System.Linq;
using Content.Shared.SimpleStation14.Announcements.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests.SimpleStation14.Announcers;

[TestFixture]
[TestOf(typeof(AnnouncerPrototype))]
public sealed class AnnouncerPrototypeTest
{
    [Test]
    public async Task TestAnnouncerFallbacks()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;

        var prototype = server.ResolveDependency<IPrototypeManager>();

        await server.WaitAssertion(() =>
        {
            foreach (var announcer in prototype.EnumeratePrototypes<AnnouncerPrototype>())
            {
                Assert.That(announcer.AnnouncementPaths.Any(a => a.ID.ToLower() == "fallback"),
                    Is.True,
                    $"Announcer \"{announcer.ID}\" does not have a fallback announcement");
            }
        });

        await pair.CleanReturnAsync();
    }
}
