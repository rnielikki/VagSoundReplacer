namespace SoundReplacer
{
    /// <summary>
    /// Metadata of one sound.
    /// </summary>
    public record VagDataBlock
    {
        public VagBlockInfo Info { get; init; }
        public VagWavBlock Wav { get; init; }
    }
}
