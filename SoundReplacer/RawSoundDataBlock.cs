namespace SoundReplacer
{
    /// <summary>
    /// Controls pure wav file data. Existing for read-only. Must be immutable.
    /// </summary>
    public class RawSoundDataBlock
    {
        private readonly byte[] _head = new byte[4] { 0x53, 0x47, 0x58, 0x44};
        private readonly string _path;
        private readonly long _start;
        private readonly int _length;

        /// <summary>
        /// RawSoundDataBlock from external file.
        /// </summary>
        /// <param name="path">The path of the RAW file.</param>
        /// <exception cref="FormatException">The file contains SGXD header (The file must be pure RAW file).</exception>
        public RawSoundDataBlock(string path)
        {
            _path = path;
            using var fileStream = File.OpenRead(path);
            var bytes = new byte[4];
            fileStream.Read(bytes);
            if (bytes.SequenceEqual(_head))
            {
                throw new FormatException("The format must be RAW without sgxd header.");
            }
            _start = 0;
            _length = (int)fileStream.Length;
        }
        /// <summary>
        /// RawSoundDataBlock from input SGXD file.
        /// </summary>
        /// <param name="start">Start position in the file.</param>
        /// <param name="length">The sound data length.</param>
        /// <exception cref="ArgumentException">Start position is zero (which isn't possible for proper VAG file).</exception>
        internal RawSoundDataBlock(long start, int length)
        {
            if (start < 1)
            {
                throw new ArgumentException("Start position cannot be zero or less without providing path");
            }
            _path = "";
            _start = start;
            _length = length;
        }

        internal byte[] ReadWav(BinaryDataReader dataReader)
        {
            if (_start == 0)
            {
                using var fileStream = File.OpenRead(_path);
                using var reader = new BinaryReader(fileStream);
                fileStream.Position = 0;
                return reader.ReadBytes((int)fileStream.Length);
            }
            else
            {
                var fileStream = dataReader.Stream;
                var reader = dataReader.Reader;
                fileStream.Position = _start;
                return reader.ReadBytes(_length);
            }
        }
    }
}
