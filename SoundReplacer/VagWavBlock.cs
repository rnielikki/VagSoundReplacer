namespace SoundReplacer
{
    /// <summary>
    /// Contains wav information, including metadata and wav address.
    /// </summary>
    public class VagWavBlock
    {
        public bool IsReplaced { get; private set; }
        public RawSoundDataBlock SoundDataBlock { get; private set; }
        public int Bitrate { get; init; }
        public int PanLeft { get; init; }
        public int PanRight { get; init; }

        internal byte[] GetWav() => SoundDataBlock.ReadWav();
        internal VagWavBlock(BinaryDataReader reader, long start, long end, int length, int bitrate, int panleft, int panright)
        {
            Bitrate = bitrate;
            PanLeft = panleft;
            PanRight = panright;
            SoundDataBlock = new RawSoundDataBlock(
                    dataReader: reader,
                    start: start,
                    end: end,
                    length: length
                    );
        }
        public void Replace(RawSoundDataBlock block)
        {
            SoundDataBlock = block;
            IsReplaced = true;
        }
    }
}
