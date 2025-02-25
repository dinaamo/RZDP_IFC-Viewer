﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using RZDP_IFC_Viewer.IFC.Model;
using RZDP_IFC_Viewer.IFC.ModelItem;
using RZDP_IFC_Viewer.Infracrucrure.Commands;
using RZDP_IFC_Viewer.IFC.Editor.Base;
using RZDP_IFC_Viewer.IFC.Model.ModelObjectPropertySet.Base;
using Xbim.Ifc2x3.PropertyResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Common;

namespace Editor_IFC
{
    public abstract class BasePropertySetDefinition : BaseItemModel, INotifyPropertyChanged
    {
        private readonly IIfcObjectDefinition ifcObjectDefinition;
        public  IIfcPropertySetDefinition IFCPropertySetDefinition { get; }
        public ModelItemIFCObject ModelObject { get; }


        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
            }
        }

        protected BasePropertySetDefinition(IIfcObjectDefinition ifcObjectDefinition, IIfcPropertySetDefinition ifcPropertySetDef, ModelIFC modelIFC, ModelItemIFCObject modelObject) : base(modelIFC)
        {
            this.ifcObjectDefinition = ifcObjectDefinition;
            this.IFCPropertySetDefinition = ifcPropertySetDef;
            this.ModelObject = modelObject;


        }

        protected abstract IEnumerable<IPropertyModel<IIfcResourceObjectSelect>> FillCollectionProperty();

        public ObservableCollection<IPropertyModel<IIfcResourceObjectSelect>> PropertyCollection => new ObservableCollection<IPropertyModel<IIfcResourceObjectSelect>>(FillCollectionProperty());

        public void DeletePropertyModel(IPropertyModel<IIfcResourceObjectSelect> propertyModel)
        {
            ModelObject.ModelIFC.DeleteIFCEntity(new List<IPersistEntity>() { propertyModel.Property });
            OnPropertyChanged("PropertyCollection");
        }

        public int CountRelatedObjectsInstanse => IFCPropertySetDefinition?.DefinesOccurrence.SelectMany(it => it.RelatedObjects).Count() ?? 0;

        public int CountRelatedObjectsType => IFCPropertySetDefinition?.DefinesType.Count() ?? 0;

        public string NamePropertySet
        {
            get
            {
                return IFCPropertySetDefinition.Name;
            }
            set
            {
                ModelIFC.ChangeName(new List<(IIfcRoot, string)> { (IFCPropertySetDefinition, value) });
                OnPropertyChanged("NamePropertySet");
                //ModelObject.OnPropertyChanged("CollectionPropertySet");
            }
        }

        public static BasePropertySetDefinition CreateEditorPropertySet(IIfcObjectDefinition ifcObjectDefinition,
                                IIfcPropertySetDefinition ifcPropertySetDefinition, ModelIFC modelIFC, ModelItemIFCObject modelItemIFCObject)
        {
            if (ifcPropertySetDefinition is Xbim.Ifc2x3.Kernel.IfcPropertySet ifcPropertySet2x3)
            {
                return new EdPropertySet2x3(ifcObjectDefinition, ifcPropertySet2x3, modelIFC, modelItemIFCObject);
            }
            else if (ifcPropertySetDefinition is Xbim.Ifc2x3.ProductExtension.IfcElementQuantity ifcElementQuantity2x3)
            {
                return new EdElementQuantity2x3(ifcObjectDefinition, ifcElementQuantity2x3, modelIFC, modelItemIFCObject);
            }
            else if (ifcPropertySetDefinition is Xbim.Ifc4.Kernel.IfcPropertySet ifcPropertySet4)
            {
                return new EdPropertySet4(ifcObjectDefinition, ifcPropertySet4, modelIFC, modelItemIFCObject);
            }
            else if (ifcPropertySetDefinition is Xbim.Ifc4.ProductExtension.IfcElementQuantity ifcElementQuantity4)
            {
                return new EdElementQuantity4(ifcObjectDefinition, ifcElementQuantity4, modelIFC, modelItemIFCObject);
            }
            else
            {
                throw new ArgumentException($"Не соответствие схемы ifc для набора характеристик");
            }
        }

        public abstract void AddProperty(string nameProperty, string valueProperty);

        public abstract void CreateNewProperty();

        public abstract IIfcPropertySetDefinition GetCopyPropertySet();

        public override bool Equals(object? other)
        {
            if (other is BasePropertySetDefinition otherPropSet)
            {
                return Equals(otherPropSet.IFCPropertySetDefinition, IFCPropertySetDefinition);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return IFCPropertySetDefinition.GetHashCode();
        }
    }
}