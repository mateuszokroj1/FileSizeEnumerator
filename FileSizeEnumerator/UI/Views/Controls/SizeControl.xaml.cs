using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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
            var model = new SizeControlViewModel();
            DataContext = model;

            model.PropertyChangedObservable.Where(name => name == "Length").Subscribe(name => Size = model.Length);
        }

        private const ulong InitialValue = 0;

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register
            (
                "Size",
                typeof(ulong),
                typeof(SizeControl),
                new PropertyMetadata(InitialValue, SizeProperty_Changed)
            );

        public ulong Size
        {
            get => (ulong)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        private static void SizeProperty_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is Control control && control.DataContext is SizeControlViewModel model)
                if (!Equals(model.Length, e.NewValue))
                    model.Length = (ulong)e.NewValue;
        }
    }
}
