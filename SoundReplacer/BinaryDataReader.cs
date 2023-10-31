using System.Text;
using System.Security.Cryptography;

namespace SoundReplacer
{
    /// <summary>
    /// Opens param file for reading.
    /// </summary>
    internal class BinaryDataReader : IDisposable
    {
        public FileStream Stream { get; }
        public BinaryReader Reader { get; }
        internal long DataLength { get; }
        /// <summary>
        /// One data collection size, set by <see cref="ParamFileParser"/>.
        /// </summary>
        internal int CollectionSize { get; set; }
        /// <summary>
        /// Opens the binary file.
        /// </summary>
        /// <param name="filePath">path of the file</param>
        internal BinaryDataReader(string filePath)
        {
            Stream = File.OpenRead(filePath);
            Reader = new BinaryReader(Stream);
            DataLength = Stream.Length;
        }
        internal sbyte GetByte(long offset)
        {
            Stream.Position = offset;
            return Reader.ReadSByte();
        }
        internal short GetShort(long offset)
        {
            Stream.Position = offset;
            return Reader.ReadInt16();
        }
        internal int GetInt(long offset)
        {
            Stream.Position = offset;
            return Reader.ReadInt32();
        }
        internal float GetFloat(long offset)
        {
            Stream.Position = offset;
            return Reader.ReadSingle();
        }
        internal string GetString(long offset)
        {
            Stream.Position = offset;
            char c;
            StringBuilder result = new StringBuilder();
            while((c = (char)Reader.ReadByte())!='\0')
            {
                result.Append(c);
            }
            return result.ToString();
        }
        internal byte[] GetBytes(long start, int length)
        {
            Stream.Position = start;
            return Reader.ReadBytes(length);
        }
        internal bool IsEOF() => Stream.Position > Stream.Length - CollectionSize;
        public void Dispose()
        {
            Reader.Dispose();
            Stream.Dispose();
        }

    }
}
