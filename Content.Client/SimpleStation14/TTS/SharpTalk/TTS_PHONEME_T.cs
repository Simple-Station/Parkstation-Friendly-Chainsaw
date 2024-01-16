﻿using System.Runtime.InteropServices;

namespace Content.Client.SimpleStation14.TTS.SharpTalk
{
    [StructLayout(LayoutKind.Sequential)]
    // ReSharper disable once InconsistentNaming
    struct TTS_PHONEME_T
    {
        public uint Phoneme;
        public uint PhonemeSampleNumber;
        public uint PhonemeDuration;
        private readonly uint _reserved;
    }
}
