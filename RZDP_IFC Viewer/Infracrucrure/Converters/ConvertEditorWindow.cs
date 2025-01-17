using System.Globalization;
using System.Windows.Data;
using Editor_IFC;

namespace IFC_Table_View.Infracrucrure.Converter
{
    /// <summary>
    /// Возвращаем тип данных
    /// </summary>
    //public class ConvertItemPropertiesDataType : IValueConverter
    //{
    //    public object Convert(object valueObject, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        //if (valueObject is IIfcPropertySingleValue ifcValue)
    //        //{
    //        //    return ifcValue?.NominalValue?.UnderlyingSystemType.Name;
    //        //}
    //        //else if (valueObject is IIfcPropertyReferenceValue ifcRefValue)
    //        //{
    //        //    if (ifcRefValue.PropertyReference is IIfcTable ifcTable)
    //        //    {
    //        //        return "IFCTable";
    //        //    }
    //        //    else if (ifcRefValue.PropertyReference is IIfcDocumentReference ifcDocumentReference)
    //        //    {
    //        //        return "IFCDocumentReference";
    //        //    }
    //        //}
    //        //else if (valueObject is IIfcQuantityArea quantityArea)
    //        //{
    //        //    return quantityArea.AreaValue.Value.GetType().Name;
    //        //}
    //        //else if (valueObject is IIfcQuantityCount quantityCount)
    //        //{
    //        //    return quantityCount.CountValue.Value.GetType().Name;
    //        //}
    //        //else if (valueObject is IIfcQuantityLength quantityLength)
    //        //{
    //        //    return quantityLength.LengthValue.Value.GetType().Name;
    //        //}
    //        //else if (valueObject is IIfcQuantityTime quantityTime)
    //        //{
    //        //    return quantityTime.TimeValue.Value.GetType().Name;
    //        //}
    //        //else if (valueObject is IIfcQuantityVolume quantityVolume)
    //        //{
    //        //    return quantityVolume.VolumeValue.Value.GetType().Name;
    //        //}
    //        //else if (valueObject is IIfcQuantityWeight quantityWeight)
    //        //{
    //        //    return quantityWeight.WeightValue.Value.GetType().Name;
    //        //}

    //        //return null;

    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return null;
    //    }
    //}

    public class ConvertItemGetCollectionProperty : IValueConverter
    {
        public object Convert(object valueObject, Type targetType, object parameter, CultureInfo culture)
        {
            if (valueObject is BasePropertySetDefinition propertySetDefinition)
            {
                return propertySetDefinition.PropertyCollection;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}