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
    public class ParametersObjectToExportOrImport
    {
        public string ElementName { get; set; }
        public string GUUID { get; set; }
        public string IFCClass { get; set; }
        public List<PropertySetToExportOrImport> PropertySets { get; set; }

        public ParametersObjectToExportOrImport()
        { }

        public ParametersObjectToExportOrImport(ModelItemIFCObject modelItemObject)
        {
            ElementName = modelItemObject.IFCObjectName;
            GUUID = modelItemObject.IFCObjectGUID;
            IFCClass = modelItemObject.IFCClass;
            PropertySets = modelItemObject.CollectionPropertySet.Select(it => new PropertySetToExportOrImport(it)).ToList();
        }
    }


    [Serializable]
    public class PropertySetToExportOrImport
    {
        public string NamePropertySetToExport { get; set; }
        public List<PropertyToExportOrImport> PropertyToExportCollection { get; set; }

        public PropertySetToExportOrImport()
        { }

        public PropertySetToExportOrImport(BasePropertySetDefinition propertySet)
        {
            NamePropertySetToExport = propertySet.NamePropertySet;
            PropertyToExportCollection = propertySet.PropertyCollection.Select(it => new PropertyToExportOrImport(it)).ToList();
        }

    }

    [Serializable]
    public class PropertyToExportOrImport
    {
        public string DataType { get; set; }
        public string NameProperty { get; set; }
        public string ValueString { get; set; }


        public PropertyToExportOrImport()
        { }

        public PropertyToExportOrImport(IPropertyModel<IIfcResourceObjectSelect> propertyModel)
        {
            DataType = propertyModel.DataType;
            NameProperty = propertyModel.NameProperty;
            ValueString = propertyModel.ValueString;
        }
    }
}
