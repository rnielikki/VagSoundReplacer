namespace SoundReplacer
{
    /// <summary>
    /// Metadata of one sound.
    /// </summary>
    public record VagSoundBlock
    {
        public string Name { get; init; }
        public VagWavBlock Wav { get; init; }
    }
}
