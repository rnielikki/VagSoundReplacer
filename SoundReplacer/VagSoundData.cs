using Microsoft.VisualBasic;

namespace SoundReplacer
{
    /// <summary>
    /// Contains original sound data and its metadata.
    /// </summary>
    internal class VagSoundData : IDisposable
    {
        private readonly BinaryDataReader _dataReader;
        private readonly MetadataSearcher _metaSearcher;
        private readonly string _inputPath;
        private readonly string _fileName;
        private VagSoundBlock[] _blocks;
        private int _wavStartOffset;
        private long _wavHeaderAddr;
        private int _wavAmount;
        public VagSoundBlock[] Blocks
        {
            get
            {
                if (_blocks == null) Init();
                return _blocks;
            }
        }
        internal VagSoundData(string path)
        {
            _dataReader = new BinaryDataReader(path);
            _metaSearcher = new MetadataSearcher(_dataReader);
            _inputPath = path;
            _fileName = Path.GetFileNameWithoutExtension(path);
        }
        public void Init()
        {
            _wavStartOffset = _dataReader.GetInt(0x8);
            _wavHeaderAddr = _metaSearcher.SearchFor(new byte[] { 0x57, 0x41, 0x56, 0x45 });
            if (_wavHeaderAddr < 0)
            {
                throw new FormatException("[MetadataSearcher] Cannot find the header. Check if input file is correct.");
            }
            else if (_wavHeaderAddr == 0x10)
            {
                throw new FormatException("[MetadataSearcher] The target file looks like ATRAC format. Please use another method to apply your music :)");
            }
            _wavAmount = _dataReader.GetInt(_wavHeaderAddr + 0xc);
            _wavHeaderAddr += 0x10;
            _blocks = new VagSoundBlock[_wavAmount];

            var nameHeaderAddr = _metaSearcher.SearchFor(new byte[] { 0x4E, 0x41, 0x4D, 0x45 });
            var nameAmount = _dataReader.GetInt(nameHeaderAddr + 0xc);
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
                    var kv = GetName(nameHeaderAddr + i * 0x8);
                    if (!names.ContainsKey(kv.Key))
                    {
                        names.Add(kv.Key, kv.Value);
                    }
                }
            }
            for (short i = 0; i < _wavAmount; i++)
            {
                _blocks[i] = new VagSoundBlock
                {
                    Name = names.ContainsKey(i) ? names[i] : $"{_fileName}#{i}",
                    Wav = GetWavBlocks(_wavHeaderAddr + i * 0x38)
                };
            }
        }
        public bool Replace(int index, string path)
        {
            if (index < 0 || index >= _wavAmount)
            {
                return false;
            }
            _blocks[index].Wav.Replace(
                new RawSoundDataBlock(path)
                );
            return true;
        }
        internal void Write(string outputPath)
        {
            if (_inputPath == outputPath)
            {
                throw new ArgumentException("output path cannot be same as input path");
            }
            using var file = File.Open(outputPath, FileMode.Create);
            //prepare for each index
            int[] addrs = new int[_wavAmount];
            int currentOffset = 0;

            //1. load bytes of the array
            file.Write(_dataReader.GetBytes(0x0, _wavStartOffset));
            //2. put wav files
            for (int i = 0; i < _wavAmount; i++)
            {
                var wav = _blocks[i].Wav;
                file.Write(wav.GetWav());
                currentOffset += wav.SoundDataBlock.Length;
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
        private VagWavBlock GetWavBlocks(long offset) =>
                new VagWavBlock(
                    reader: _dataReader,
                    start: (long)_dataReader.GetInt(offset + 0x30) + _wavStartOffset,
                    end: (long)_dataReader.GetInt(offset + 0x34) + _wavStartOffset,
                    length: _dataReader.GetInt(offset + 0x2C),
                    panleft: _dataReader.GetShort(offset + 0x18),
                    panright: _dataReader.GetShort(offset + 0x1A),
                    bitrate: _dataReader.GetInt(offset + 0xC)
                    );
        private KeyValuePair<short, string> GetName(long offset) =>
            new KeyValuePair<short, string>(
                _dataReader.GetShort(offset),
                _dataReader.GetString(
                    _dataReader.GetInt(offset + 0x4)
                )
            );

        public void Dispose()
        {
            _metaSearcher.Dispose();
            _dataReader.Dispose();
        }
    }
}
