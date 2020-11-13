using System;
using System.Globalization;
using System.Windows.Data;

namespace FileSizeEnumerator.UI
{
    public class SizeNumberToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double size;
            try
            {
                size = System.Convert.ToDouble(value);
            }
            catch(InvalidCastException)
            {
                return "Invalid type to convert.";
            }

            if (size < 1000)
                return $"{Math.Max(0, size)} B";
            else if (size >= 1000 && size < 1_000_000)
                return $"{Math.Round(size / 1000, 2)} kB";
            else if (size >= 1_000_000 && size < 1_000_000_000)
                return $"{Math.Round(size / 1_000_000, 2)} MB";
            else if (size >= 1_000_000_000 && size < 1_000_000_000_000)
                return $"{Math.Round(size / 1_000_000_000, 2)} GB";
            else
                return $"{Math.Round(size / 1_000_000_000_000, 2)} TB";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString();
        }
    }
}
