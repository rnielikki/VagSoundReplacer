namespace SoundReplacer
{
    public class NameAliasManager
    {
        private const string _namePath = "name.txt";
        private readonly Dictionary<string, VagBlockInfo> _names = new();
        public NameAliasManager()
        {
            if (!File.Exists(_namePath))
            {
                return;
            }
            using var nameStream = File.OpenRead(_namePath);
            using var nameReader = new StreamReader(nameStream);
            string line;
            int lineNumber = 1;
            while ((line = nameReader.ReadLine()) != null)
            {
                if (!line.StartsWith("# ") && !string.IsNullOrWhiteSpace(line))
                {
                    var info = ParseLine(line,lineNumber);
                    _names.Add(info.OriginalName, info);
                }
                lineNumber++;
            }
        }
        private VagBlockInfo ParseLine(string line, int lineNumber)
        {
            var splitted = line.Split("\t");
            if (splitted.Length != 3)
            {
                throw new FormatException($"[{nameof(NameAliasManager)}] Setting file error (at line {lineNumber}) : One data must have exactly 3 sections separated by tab");
            }
            if (!int.TryParse(splitted[2].Trim(), out int sampleRate))
            {
                throw new FormatException($"[{nameof(NameAliasManager)}] Setting file error (at line {lineNumber}) : The last section must be valid integer (real sample rate).");
            }
            var originalName = splitted[0].Trim();
            var name = splitted[1].Trim();
            return new VagBlockInfo
            {
                OriginalName = originalName,
                Name = name,
                RealSampleRate = sampleRate
            };
        }
        public VagBlockInfo GetBlockInfo(string realName)
        {
            if (_names.TryGetValue(realName, out VagBlockInfo info))
            {
                return info;
            }
            return new VagBlockInfo
            {
                Name = realName,
                OriginalName = realName,
                RealSampleRate = -1
            };
        }
    }
}
