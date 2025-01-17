using System.Globalization;
using System.Windows.Data;
using IFC_Table_View.IFC.ModelItem;

namespace IFC_Table_View.Infracrucrure.Converter
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