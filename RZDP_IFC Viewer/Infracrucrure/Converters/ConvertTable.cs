using System.Globalization;
using System.Windows.Data;
using RZDP_IFC_Viewer.IFC.ModelItem;

namespace RZDP_IFC_Viewer.Infracrucrure.Converter
{
    public class ConvertItemTable : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ModelItemIFCTable modelTable)
            {
                return modelTable.dataTable;
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}