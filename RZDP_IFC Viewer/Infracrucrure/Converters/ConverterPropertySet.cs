using System.Globalization;
using System.Windows.Data;
using IFC_Table_View.IFC.ModelItem;
using Xbim.Ifc4.Interfaces;

namespace IFC_Table_View.Infracrucrure.Converter
{
    // <summary>
    /// Возвращаем наименование характеристики
    /// </summary>
    public class ConvertItemIFCNameProperty : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IIfcProperty ifcProperty)
            {
                return ifcProperty.Name;
            }
            else if (value is IIfcPhysicalQuantity ifcPhysicalQuantity)
            {
                return ifcPhysicalQuantity.Name;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// Возвращаем значение характеристики
    /// </summary>
    public class ConvertItemIFCValue : IValueConverter
    {
        public object Convert(object valueObject, Type targetType, object parameter, CultureInfo culture)
        {
            if (valueObject is IIfcPropertySingleValue ifcValue)
            {
                return System.Convert.ToString(ifcValue?.NominalValue?.Value);
            }
            else if (valueObject is IIfcPropertyReferenceValue ifcRefValue)
            {
                if (ifcRefValue.PropertyReference is IIfcTable ifcTable)
                {
                    return System.Convert.ToString(ifcRefValue.PropertyReference);
                }
                else if (ifcRefValue.PropertyReference is IIfcDocumentReference ifcDocumentReference)
                {
                    return ifcDocumentReference.ReferencedDocument?.Identification;
                }
            }
            else if (valueObject is IIfcQuantityArea quantityArea)
            {
                return System.Convert.ToString(quantityArea.AreaValue);
            }
            else if (valueObject is IIfcQuantityCount quantityCount)
            {
                return System.Convert.ToString(quantityCount.CountValue);
            }
            else if (valueObject is IIfcQuantityLength quantityLength)
            {
                return System.Convert.ToString(quantityLength.LengthValue);
            }
            else if (valueObject is IIfcQuantityTime quantityTime)
            {
                return System.Convert.ToString(quantityTime.TimeValue);
            }
            else if (valueObject is IIfcQuantityVolume quantityVolume)
            {
                return System.Convert.ToString(quantityVolume.VolumeValue);
            }
            else if (valueObject is IIfcQuantityWeight quantityWeight)
            {
                return System.Convert.ToString(quantityWeight.WeightValue);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ConvertItemPropertiesIFC : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //if (value is IIfcPropertySet PropertySet)
            //{
            //    return PropertySet.HasProperties;
            //}
            //else if (value is IIfcElementQuantity PropertySetQuantity)
            //{
            //    return PropertySetQuantity.Quantities;
            //}
            //else
            //{
            //    return null;
            //}
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ConvertItemPropSetIFC : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ModelItemIFCObject element)
            {
                return element.CollectionPropertySet;
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

    public class ConvertItemStatusClassIFC : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BaseModelItemIFC modelItem)
            {
                return modelItem.IFCClass;
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

    public class ConvertItemStatusGUID : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ModelItemIFCObject modelObject)
            {
                return modelObject.IFCObjectGUID;
            }
            else
            {
                return "-";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ConvertItemTypeClassIFC : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.GetType().Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}