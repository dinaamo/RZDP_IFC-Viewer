using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Editor_IFC;
using RZDP_IFC_Viewer.IFC.Model.ModelObjectPropertySet.Base;
using RZDP_IFC_Viewer.IFC.ModelItem;
using Xbim.Ifc4.Interfaces;

namespace RZDP_IFC_Viewer.Exporter
{
    [Serializable]
    public class ParametersObjectProvider
    {
        public string ElementName { get; set; }
        public string GUUID { get; set; }
        public string IFCClass { get; set; }
        public List<PropertySetProvider> PropertySets { get; set; }

        public ParametersObjectProvider()
        { }

        public ParametersObjectProvider(ModelItemIFCObject modelItemObject)
        {
            ElementName = modelItemObject.IFCObjectName;
            GUUID = modelItemObject.IFCObjectGUID;
            IFCClass = modelItemObject.IFCClass;
            PropertySets = modelItemObject.CollectionPropertySet.Select(it => new PropertySetProvider(it)).ToList();
        }
    }


    [Serializable]
    public class PropertySetProvider
    {
        public string NamePropertySetProvider { get; set; }
        public List<PropertyProvider> PropertyProvider { get; set; }

        public PropertySetProvider()
        { }

        public PropertySetProvider(BasePropertySetDefinition propertySet)
        {
            NamePropertySetProvider = propertySet.NamePropertySet;
            PropertyProvider = propertySet.PropertyCollection.Select(it => new PropertyProvider(it)).ToList();
        }

    }

    [Serializable]
    public class PropertyProvider
    {
        public string DataType { get; set; }
        public string NamePropertyProvider { get; set; }
        public string ValueString { get; set; }


        public PropertyProvider()
        { }

        public PropertyProvider(IPropertyModel<IIfcResourceObjectSelect> propertyModel)
        {
            DataType = propertyModel.DataType;
            NamePropertyProvider = propertyModel.NameProperty;
            ValueString = propertyModel.ValueString;
        }
    }
}
