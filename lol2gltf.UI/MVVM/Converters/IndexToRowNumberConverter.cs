using System;
using System.Globalization;
using System.Windows.Data;

namespace lol2gltf.UI.MVVM.Converters
{
    public class IndexToRowNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value + 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
