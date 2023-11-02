using SoundReplacer;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class FileSelector : UserControl
    {
        private string _label;
        private Brush _errorColor;
        public string Label
        {
            get => _label;
            private set
            {
                _label = value;
                TextLabel.Content = value;
            }
        }
        public bool HasValidFile { get; private set; }
        private VagDataBlock _data;
        public VagDataBlock Data {
            get => _data;
            set
            {
                _data = value;
                var sampleRate = value.Info.RealSampleRate;
                Label = value.Info.Name;
                if (sampleRate < 0)
                {
                    RateLabel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    RateLabel.Content = $"({sampleRate} Hz)";
                }
            }
        }

        public string FileName { get; private set; } = "";

        public FileSelector()
        {
            InitializeComponent();
            _errorColor = new SolidColorBrush(Color.FromRgb(255, 150, 150));
        }
        private void GetFile(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".raw";
            dialog.Filter = "RAW files (.raw)|*.raw";
            dialog.CheckFileExists = true;
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                try
                {
                    _data.Wav.Replace(
                        new RawSoundDataBlock(dialog.FileName)
                        );
                    string fileName = dialog.FileName;
                    PathViewer.Text = fileName;
                    FileName = fileName;
                }
                catch(System.Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                }
            }
        }
    }
}
