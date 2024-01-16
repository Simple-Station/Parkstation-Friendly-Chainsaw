namespace Content.Client.SimpleStation14.TTS.SharpTalk
{
    /// <summary>
    /// Holds information about phoneme events fired by the TTS engine.
    /// </summary>
    public class PhonemeEventArgs : EventArgs
    {
        /// <summary>
        /// Indicates the phoneme being spoken.
        /// </summary>
        public readonly char Phoneme;
        /// <summary>
        /// The duration of the phoneme in milliseconds.
        /// </summary>
        public readonly uint Duration;

        internal PhonemeEventArgs(char phoneme, uint duration)
        {
            this.Phoneme = phoneme;
            this.Duration = duration;
        }
    }
}
