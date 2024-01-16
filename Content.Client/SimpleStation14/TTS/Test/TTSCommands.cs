using JetBrains.Annotations;
using Robust.Shared.Console;
using SharpTalk;

namespace Content.Client.SimpleStation14.TTS.Test;

[UsedImplicitly]
internal sealed class TTSSayCommand : IConsoleCommand
{
    public string Command => "ttssay";
    public string Description => "Says something.";
    public string Help => $"{Command} <message>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length == 0)
        {
            shell.WriteLine(Help);
            return;
        }

        var tts = new FonixTalkEngine();
        tts.Voice = TtsVoice.Frank;
        tts.Speak(args[0]);
    }
}
