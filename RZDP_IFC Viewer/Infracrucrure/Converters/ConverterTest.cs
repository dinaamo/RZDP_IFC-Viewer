using System.Globalization;
using System.Windows.Data;

namespace RZDP_IFC_Viewer.Infracrucrure.Converter
{
    public class ConvertTest : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}