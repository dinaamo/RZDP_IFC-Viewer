﻿using System.Globalization;
using System.Windows.Data;

namespace RZDP_IFC_Viewer.Infracrucrure.Converter
{
    internal class ConvertParametersMultibuilding : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.ToArray();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}