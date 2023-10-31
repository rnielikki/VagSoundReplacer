namespace SoundReplacer
{
    /// <summary>
    /// Controls pure wav file data. Existing for read-only. Must be immutable.
    /// </summary>
    public class RawSoundDataBlock : IDisposable
    {
        private readonly byte[] _head = new byte[4] { 0x53, 0x47, 0x58, 0x44};
        private readonly FileStream _fileStream;
        private readonly BinaryReader _reader;
        public long Start { get; }
        public long End { get; }
        public int Length { get; }

        public RawSoundDataBlock(string path)
        {
            _fileStream = File.OpenRead(path);
            _reader = new BinaryReader(_fileStream);
            Start = 0;
            Length = (int)_fileStream.Length;
            End = Length;

            var bytes = new byte[4];
            _fileStream.Read(bytes);
            if (bytes.SequenceEqual(_head))
            {
                throw new FormatException("The format must be RAW without sgxd header.");
            }
        }
        internal RawSoundDataBlock(BinaryDataReader dataReader, long start, long end, int length)
        {
            _fileStream = dataReader.Stream;
            _reader = dataReader.Reader;
            Start = start;
            End = end;
            Length = length;
        }

        public void Dispose()
        {
            _fileStream.Dispose();
        }

        internal byte[] ReadWav()
        {
            _fileStream.Position = Start;
            return _reader.ReadBytes(Length);
        }
    }
}
