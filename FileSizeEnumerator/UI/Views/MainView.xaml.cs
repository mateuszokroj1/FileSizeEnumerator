using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Data;

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

            var view = CollectionViewSource.GetDefaultView(List.ItemsSource);
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription
            {
                Direction = ListSortDirection.Descending,
                PropertyName = "Length"
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var model = DataContext as MainViewModel;
            model?.Dispose();
        }
    }
}
