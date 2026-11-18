namespace UniCortex.Editor.Domains.Models
{
    // Group parameters that can be exposed with expose_audio_mixer_parameter.
    public static class AudioMixerParameters
    {
        public const string Volume = "volume";
        public const string Pitch = "pitch";

        // Reported by get_audio_mixer for exposed parameters that are neither of the above
        // (e.g. effect parameters exposed in the Audio Mixer window).
        public const string Other = "other";
    }
}
