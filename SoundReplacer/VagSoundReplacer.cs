namespace SoundReplacer
{
    public class VagSoundReplacer
    {
        private readonly string _path;
        private VagSoundData _soundData;
        public VagSoundReplacer(string path)
        {
            _path = path;
            _soundData = new VagSoundData(path);
        }
        public VagSoundBlock[] GetSoundData() 
        {
            return _soundData.Blocks;
        }
        public bool ReplaceOne(int index, string path)
        {
            return _soundData.Replace(index, path);
        }
        public void Write(string outputPath)
        {
            _soundData.Write(outputPath);
        }
    }
}