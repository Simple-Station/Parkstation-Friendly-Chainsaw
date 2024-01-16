using System.Runtime.InteropServices;

namespace Content.Client.SimpleStation14.TTS.SharpTalk
{
    [StructLayout(LayoutKind.Sequential)]
    // ReSharper disable once InconsistentNaming
    struct TTS_INDEX_T
    {
        public uint IndexValue;
        public uint SampleNumber;
        readonly uint _reserved;
    }
}
