using System.Diagnostics;
using System.IO;
using System.Windows;

namespace FileSizeEnumerator.UI
{
    /// <summary>
    /// Represents main window of app
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var model = DataContext as MainViewModel;
            model?.Dispose();
        }
    }
}
