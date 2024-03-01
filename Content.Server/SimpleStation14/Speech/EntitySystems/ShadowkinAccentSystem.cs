using System.Text.RegularExpressions;
using Content.Server.Speech;
using Content.Server.Speech.Components;
using Robust.Shared.Random;

namespace Content.Server.SimpleStation14.Speech.EntitySystems
{
    public sealed class ShadowkinAccentSystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;

        private static readonly Regex mRegex = new(@"[adgjmpsv]", RegexOptions.Compiled);
        private static readonly Regex aRegex = new(@"[behknqtwy]", RegexOptions.Compiled);
        private static readonly Regex rRegex = new(@"[cfiloruxz]", RegexOptions.Compiled);
        private static readonly Regex MRegex = new(@"[ADGJMPSV]", RegexOptions.Compiled);
        private static readonly Regex ARegex = new(@"[BEHKNQTWY]", RegexOptions.Compiled);
        private static readonly Regex RRegex = new(@"[CFILORUXZ]", RegexOptions.Compiled);


        public override void Initialize()
        {
            SubscribeLocalEvent<ShadowkinAccentComponent, AccentGetEvent>(OnAccent);
        }


        public string Accentuate(string message)
        {
            message = message.Trim();

            // Replace letters with other letters
            message = mRegex.Replace(message, "m");
            message = aRegex.Replace(message, "a");
            message = rRegex.Replace(message, "r");
            message = MRegex.Replace(message, "M");
            message = ARegex.Replace(message, "A");
            message = RRegex.Replace(message, "R");

            return message;
        }

        private void OnAccent(EntityUid uid, ShadowkinAccentComponent component, AccentGetEvent args)
        {
            args.Message = Accentuate(args.Message);
        }
    }
}
