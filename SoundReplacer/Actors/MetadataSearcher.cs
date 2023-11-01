namespace SoundReplacer
{
    internal class MetadataSearcher : IDisposable
    {
        private const int MAX_DEPTH = 8;
        private readonly BinaryDataReader _reader;

        internal MetadataSearcher(BinaryDataReader reader)
        {
            _reader = reader;
        }


        /// <summary>
        /// Search for the start of specific address that contains specific id.
        /// </summary>
        /// <param name="bytes">byte array, possibly in string-like form, like WAVE or NAME etc.</param>
        /// <returns>Offset of corresponding metadata.</returns>
        /// <exception cref="ArgumentException">This throws exception if the argument length isn't 4.</exception>
        /// <exception cref="FormatException">The file format is incorrect, so cannot find the address.</exception>
        internal long SearchFor(byte[] bytes)
        {
            if (bytes.Length != 4)
            {
                throw new ArgumentException("[MetadataSearcher] Length must be 4.");
            }
            int depth = 0;
            long currentPos = 0x10;
            while (depth <= MAX_DEPTH && currentPos <= _reader.DataLength)
            {
                bool isIdentical = true;
                for(int i=0;i<4;i++)
                {
                    isIdentical = bytes[i] == _reader.GetByte(currentPos+i);
                    if (!isIdentical) break;
                }
                if (isIdentical)
                {
                    return currentPos;
                }
                //this is how does it find header, I swear I read the code
                currentPos += _reader.GetInt(currentPos + 0x4) + 0x8;
                if (currentPos == 0) break;
                depth++;
            }
            return -1;
        }
        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
