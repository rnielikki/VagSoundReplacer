namespace SoundReplacer
{
    public class VagSoundReplacer
    {
        private readonly VagSoundDataManager _soundData;
        public VagSoundReplacer(string path)
        {
            _soundData = new VagSoundDataManager(path);
        }
        public VagDataBlock[] GetSoundData() 
        {
            return _soundData.Blocks;
        }
        public bool ReplaceOne(int index, string path)
        {
            return _soundData.Replace(index, path);
        }
        public bool HasAnyChange()
        {
            foreach (var block in _soundData.Blocks)
            {
                if (block.Wav.IsReplaced)
                {
                    return true;
                }
            }
            return false;
        }
        public void Write(string outputPath)
        {
            _soundData.Write(outputPath);
        }
    }
}