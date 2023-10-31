namespace SoundReplacer
{
    public class VagSoundReplacer
    {
        private readonly string _path;
        private VagSoundData _data;
        public VagSoundReplacer(string path)
        {
            _path = path;
            _data = new VagSoundData(_path);
        }
        public VagSoundBlock[] GetSoundData() 
        {
            return _data.Blocks;
        }
        public bool ReplaceOne(int index, string path)
        {
            return _data.Replace(index, path);
        }
        public void Write(string outputPath)
        {
            _data.Write(outputPath);
        }
        ~VagSoundReplacer()
        {
            _data.Dispose();
        }
    }
}