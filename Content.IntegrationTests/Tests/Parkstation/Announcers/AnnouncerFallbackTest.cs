using System.Linq;
using Content.Shared.Parkstation.Announcements.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests.Parkstation.Announcers;

[TestFixture]
[TestOf(typeof(AnnouncerPrototype))]
public sealed partial class AnnouncerPrototypeTests
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
                Assert.That(announcer.Announcements.Any(a => a.ID.ToLower() == "fallback"),
                    Is.True,
                    $"Announcer \"{announcer.ID}\" does not have a fallback announcement");
            }
        });

        await pair.CleanReturnAsync();
    }
}
