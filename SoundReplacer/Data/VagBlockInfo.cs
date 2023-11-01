namespace SoundReplacer
{
    /// <summary>
    /// Name related metadata. This data must come from settings.
    /// </summary>
    public record VagBlockInfo
    {
        /// <summary>
        /// Original name in VAG file, to match alias name.
        /// </summary>
        public string OriginalName { get; init; }
        /// <summary>
        /// The alias name that defined in setting file.
        /// </summary>
        public string Name { get; init; }
        /// <summary>
        /// Real sample rate to inform user. Doesn't affect the replacing process.
        /// </summary>
        public int RealSampleRate { get; init; }
    }
}
