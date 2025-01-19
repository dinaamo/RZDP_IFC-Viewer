using System.Globalization;
using System.Windows.Data;
using IFC_Table_View.IFC.ModelItem;
using Xbim.Ifc4.Interfaces;

namespace IFC_Table_View.Infracrucrure.Converter
{
    //Имя элемента в дереве
    public class ConvertItemName : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(System.Convert.ToString(values[0])))
            {
                return values[1];
            }
            else
            {
                return values[0];
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    //Имя элемента в панели статуса
    public class ConvStatusName : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ModelItemIFCObject modelItem)
            {
                return modelItem.IFCObjectName;
            }
            else if (value is BaseModelReferenceIFC referenceObject)
            {
                return referenceObject.NameReference;
            }
            else
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    //Свойства объекта
    //public class ConvertItemIFC : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if (value is BaseModelItemIFC elementFile)
    //        {
    //            return elementFile.PropertyElement;
    //        }
    //        else
    //        {
    //            return null;
    //        }
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return null;
    //    }
    //}

    public class ConvertPropertyFileIFC : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ModelItemDocumentReference documentReference)
            {
                return documentReference.PropertyFile;
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

    public class ConvertConvPropertyItem : IValueConverter
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

    //Свойства для нижнего поля
    public class ConvertConvReferenceToObject : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ModelItemIFCObject elementObject)
            {
                return $"Guid:{elementObject.IFCObjectGUID} " +
                        $"\nКласс: {elementObject.IFCClass} " +
                        $"\nИмя: {elementObject.IFCObjectName}";
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ConvertItemTreeName : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IIfcObjectDefinition ifcObj)
            {
                if (string.IsNullOrEmpty(ifcObj.Name))
                {
                    return ifcObj.GetType().Name;
                }
                else
                {
                    return ifcObj.Name;
                }
            }
            else if (value is IIfcTable ifcTable)
            {
                return ifcTable.Name;
            }
            else if (value is IIfcDocumentReference ifcDocumentReference)
            {
                return ifcDocumentReference.Name;
            }
            else
            {
                return "?????";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}