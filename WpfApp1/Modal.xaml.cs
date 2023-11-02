using SoundReplacer;
using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Modal0 : Window
    {
        public Modal0()
        {
            InitializeComponent();
        }
        internal void Proceed(VagSoundReplacer replacer, string outputPath)
        {
            try
            {
                replacer.Write(outputPath);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
            finally
            {
                Close();
            }
            MessageBox.Show("All done!","OK");
        }
    }
}
