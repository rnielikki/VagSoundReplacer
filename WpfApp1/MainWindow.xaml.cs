using SoundReplacer;
using System;
using System.IO;
using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private VagSoundReplacer _replacer;
        private string _inputPath;
        private string _outputPath;
        public MainWindow()
        {
            InitializeComponent();
            //check args
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                OpenFile(args[1]);
            }
        }
        private void LoadSelectors(VagDataBlock[] blocks)
        {
            FileSelectorCollection.Children.Clear();
            foreach (var block in blocks)
            {
                FileSelectorCollection.Children.Add(
                    new FileSelector
                    {
                        Data = block
                    }
                );
            }
        }

        private void FileOpener_Click(object sender, RoutedEventArgs e)
        {
            ProceedOpen();
        }

        private void FileOpener_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null && files.Length > 0)
                {
                    OpenFile(files[0]);
                }
            }
        }
        private void ProceedOpen()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".sgd";
            dialog.Filter = "SGD Sound effect files (.sgd)|*.sgd";
            dialog.CheckFileExists = true;
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                OpenFile(dialog.FileName);
            }
        }
        private void OpenFile(string path)
        {
            if (!File.Exists(path)) return;
            try
            {
                _replacer = new VagSoundReplacer(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            LoadSelectors(_replacer.GetSoundData());
            Title = $"SoundReplacer - {path}";
            _inputPath = path;
            IntroInterface.Visibility = Visibility.Collapsed;
            OpenInterface.Visibility = Visibility.Visible;
        }

        private void OpenWhenOpen_Click(object sender, RoutedEventArgs e)
        {
            if (_replacer.HasAnyChange())
            {
                var msgResult = MessageBox.Show("Your changes not applied! Is it okay to close without saving?", "Unsaved changes", MessageBoxButton.YesNo);
                if (msgResult != MessageBoxResult.Yes) return;
            }
            ProceedOpen();
        }
        private void OutputPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.DefaultExt = ".sgd";
            dialog.Filter = "SGD Sound effect files (.sgd)|*.sgd";
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                var fileName = dialog.FileName;
                if (fileName == _inputPath)
                {
                    MessageBox.Show("Output path cannot be same as input path. Choose another name or new name.", "Error");
                    return;
                }
                _outputPath = fileName;
                OutputLabel.Text = fileName;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!_replacer.HasAnyChange())
            {
                MessageBox.Show("Nothing to change!", "All done");
                return;
            }
            else if (string.IsNullOrEmpty(_outputPath))
            {
                MessageBox.Show("Specify output path", "U forgor");
                return;
            }

            var replaceWindow = new Modal0();
            replaceWindow.Owner = this;
            replaceWindow.DataContext = this;
            replaceWindow.Show();
            replaceWindow.Proceed(
                _replacer, _outputPath
                );
        }

        private void ShowGuide(object sender, RoutedEventArgs e)
        {
            new Guide().ShowDialog();
        }
    }
}
