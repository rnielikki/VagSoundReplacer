using Microsoft.VisualBasic;

namespace SoundReplacer
{
    /// <summary>
    /// Contains original sound data and its metadata.
    /// </summary>
    internal class VagSoundData
    {
        private readonly string _inputPath;
        private readonly string _fileName;
        private int _wavStartOffset;
        private long _wavHeaderAddr;
        private int _wavAmount;
        private byte[] _hash;
        public VagSoundBlock[] Blocks { get; private set; }

        internal VagSoundData(string path)
        {
            _inputPath = path;
            _fileName = Path.GetFileNameWithoutExtension(path);
            Init();
        }
        public void Init()
        {
            using var dataReader = new BinaryDataReader(_inputPath);
            using var metaSearcher = new MetadataSearcher(dataReader);
            _hash = dataReader.GetHash();
            _wavStartOffset = dataReader.GetInt(0x8);
            _wavHeaderAddr = metaSearcher.SearchFor(new byte[] { 0x57, 0x41, 0x56, 0x45 });
            if (_wavHeaderAddr < 0)
            {
                throw new FormatException("[MetadataSearcher] Cannot find the header. Check if input file is correct.");
            }
            else if (_wavHeaderAddr == 0x10)
            {
                throw new FormatException("[MetadataSearcher] The target file looks like ATRAC format. Please use another method to apply your music :)");
            }
            _wavAmount = dataReader.GetInt(_wavHeaderAddr + 0xc);
            _wavHeaderAddr += 0x10;
            Blocks = new VagSoundBlock[_wavAmount];

            var nameHeaderAddr = metaSearcher.SearchFor(new byte[] { 0x4E, 0x41, 0x4D, 0x45 });
            var nameAmount = dataReader.GetInt(nameHeaderAddr + 0xc);
            nameHeaderAddr += 0x10;

            Dictionary<short, string> names = new Dictionary<short, string>();
            if (nameHeaderAddr < 0)
            {
                //auto name if name metadata doesn't exist
                for (short i = 0; i < nameAmount; i++)
                {
                    names.Add(i, $"{_fileName}#{i}");
                }
            }
            else
            {
                for (int i = 0; i < nameAmount; i++)
                {
                    var kv = GetName(nameHeaderAddr + i * 0x8, dataReader);
                    if (!names.ContainsKey(kv.Key))
                    {
                        names.Add(kv.Key, kv.Value);
                    }
                }
            }
            for (short i = 0; i < _wavAmount; i++)
            {
                Blocks[i] = new VagSoundBlock
                {
                    Name = names.ContainsKey(i) ? names[i] : $"{_fileName}#{i}",
                    Wav = GetWavBlocks(_wavHeaderAddr + i * 0x38, dataReader)
                };
            }
        }
        public bool Replace(int index, string path)
        {
            if (index < 0 || index >= _wavAmount)
            {
                return false;
            }
            Blocks[index].Wav.Replace(
                new RawSoundDataBlock(path)
                );
            return true;
        }
        internal void Write(string outputPath)
        {
            if (_inputPath == outputPath)
            {
                throw new ArgumentException("[Writing] Output path cannot be same as input path");
            }
            if (_hash == null)
            {
                throw new InvalidOperationException("[Writing] Error: The program is not prepared.");
            }
            using var dataReader = new BinaryDataReader(_inputPath);
            if (!dataReader.IsIdenticalHash(_hash))
            {
                throw new InvalidOperationException("[Writing] The input file is MODIFIED - Cannot process writing.");
            }
            using var file = File.Open(outputPath, FileMode.Create);
            //prepare for each index
            int[] addrs = new int[_wavAmount];
            int currentOffset = 0;

            //1. load bytes of the array
            file.Write(dataReader.GetBytes(0x0, _wavStartOffset));
            //2. put wav files
            for (int i = 0; i < _wavAmount; i++)
            {
                var wav = Blocks[i].Wav;
                var wavData = wav.GetWav(dataReader);
                file.Write(wavData);
                currentOffset += wavData.Length;
                addrs[i] = currentOffset;
            }
            //3. edit metadata
            file.Flush();
            using var fileWriter = new BinaryWriter(file);
            for (int i = 0; i < _wavAmount; i++)
            {
                var target = _wavHeaderAddr + i * 0x38;
                var start = i < 1 ? 0: addrs[i - 1];
                var end = addrs[i];
                file.Position = target+0x2C;
                fileWriter.Write(end-start);
                file.Position = target+0x30;
                fileWriter.Write(start);
                file.Position = target+0x34;
                fileWriter.Write(end);
            }
            fileWriter.Flush();
        }
        private VagWavBlock GetWavBlocks(long offset, BinaryDataReader dataReader) =>
                new VagWavBlock(
                    reader: dataReader,
                    start: (long)dataReader.GetInt(offset + 0x30) + _wavStartOffset,
                    end: (long)dataReader.GetInt(offset + 0x34) + _wavStartOffset,
                    length: dataReader.GetInt(offset + 0x2C),
                    panleft: dataReader.GetShort(offset + 0x18),
                    panright: dataReader.GetShort(offset + 0x1A),
                    bitrate: dataReader.GetInt(offset + 0xC)
                    );
        private KeyValuePair<short, string> GetName(long offset, BinaryDataReader dataReader) =>
            new KeyValuePair<short, string>(
                dataReader.GetShort(offset),
                dataReader.GetString(
                    dataReader.GetInt(offset + 0x4)
                )
            );
    }
}
