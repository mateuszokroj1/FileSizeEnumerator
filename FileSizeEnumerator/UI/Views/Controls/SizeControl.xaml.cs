using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FileSizeEnumerator.UI
{
    /// <summary>
    /// Logika interakcji dla klasy SizeControl.xaml
    /// </summary>
    public partial class SizeControl : UserControl
    {
        public SizeControl()
        {
            InitializeComponent();
            var binding = new Binding("Length");
            binding.Source = DataContext;
            binding.Mode = BindingMode.TwoWay;
            SetBinding(SizeProperty, binding);
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register("Size", typeof(long), typeof(SizeControl));

        public long Size
        {
            get => (long)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }
    }
}
