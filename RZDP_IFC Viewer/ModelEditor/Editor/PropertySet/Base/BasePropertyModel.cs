using System.ComponentModel;
using Editor_IFC;
using IFC_Table_View.IFC.Model;
using IFC_Table_View.IFC.ModelItem;
using RZDP_IFC_Viewer.IFC.Editor.Base;
using Xbim.Ifc4.Interfaces;

namespace RZDP_IFC_Viewer.IFC.Model.ModelObjectPropertySet.Base
{
    public abstract class BaseEditorProperty<T> : BaseModel, IPropertyModel<T>
    {
        protected BaseEditorProperty(T value, ModelIFC modelIFC, BasePropertySetDefinition propertySetDefinition) : base(modelIFC)
        {

            this.PropertySetDefinition = propertySetDefinition;
            this.Property = value;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
            }
        }

        public string NameProperty
        {
            get
            {
                if (Property is IIfcPropertySingleValue property)
                {
                    return property.Name;
                }
                else if (Property is IIfcPropertyReferenceValue ifcRefValue)
                {
                    //if (ifcRefValue.PropertyReference is IIfcTable ifcTable)
                    //{
                    //    return ifcTable.Name;
                    //}
                    //else if (ifcRefValue.PropertyReference is IIfcDocumentReference ifcDocumentReference)
                    //{
                    //    return ifcDocumentReference.Name;
                    //}
                    return ifcRefValue.Name;
                }

                throw new ArgumentException("Exception get NameProperty");
            }

            set
            {
                if (Property is IIfcProperty property)
                {
                    ModelIFC.ChangeName(new List<(IIfcProperty, string)> { (property, value) });
                    //OnPropertyChanged("Property");
                    OnPropertyChanged("NameProperty");
                    //PropertySetDefinition.ModelObject.OnPropertyChanged("CollectionPropertySet");
                }
            }
        }
        
        public BasePropertySetDefinition PropertySetDefinition { get; }
        public T Property { get; set; }

        public string DataType
        {
            get
            {
                if (Property is IIfcPropertySingleValue ifcValue)
                {
                    return ifcValue?.NominalValue?.UnderlyingSystemType.Name;
                }
                else if (Property is IIfcPropertyReferenceValue ifcRefValue)
                {
                    if (ifcRefValue.PropertyReference is IIfcTable ifcTable)
                    {
                        return "IFCTable";
                    }
                    else if (ifcRefValue.PropertyReference is IIfcDocumentReference ifcDocumentReference)
                    {
                        return "IFCDocumentReference";
                    }
                }

                return "Неизвестный тип";
            }
        }

        public string ValueString
        {
            get
            {
                if (Property is IIfcPropertySingleValue ifcValue)
                {
                    return Convert.ToString(ifcValue?.NominalValue?.Value);
                }
                else if (Property is IIfcPropertyReferenceValue ifcRefValue)
                {
                    if (ifcRefValue.PropertyReference is IIfcTable ifcTable)
                    {
                        return Convert.ToString(ifcRefValue.PropertyReference);
                    }
                    else if (ifcRefValue.PropertyReference is IIfcDocumentReference ifcDocumentReference)
                    {
                        return ifcDocumentReference.ReferencedDocument?.Identification;
                    }
                }
                throw new ArgumentException("Exception get ValueString");
            }
            set
            {
                ModelIFC.ChangeValue(new List<(Action<string>, string)> { (SetNewValue, value) });
                //OnPropertyChanged("Property");
                OnPropertyChanged("ValueString");
                OnPropertyChanged("DataType");
                //PropertySetDefinition.ModelObject.OnPropertyChanged("CollectionPropertySet");
            }
        }

        public abstract void SetNewValue(string stringValue);

        protected abstract void SetNewValueForProperty<T>(string newValueString, T simpleValue);
    }

    public abstract class BaseEditorQuantity<T> : BaseModel, IPropertyModel<IIfcPhysicalQuantity>
    {
        protected BaseEditorQuantity(IIfcPhysicalQuantity value, ModelIFC modelIFC, BasePropertySetDefinition propertySetDefinition) : base(modelIFC)
        {
            this.PropertySetDefinition = propertySetDefinition;
            this.Property = value;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
            }
        }

        public string NameProperty
        {
            get
            {
                return Property.Name;
            }
            set
            {
                ModelIFC.ChangeName(new List<(IIfcPhysicalQuantity, string)> { (Property, value) });
                //OnPropertyChanged("Property");
                OnPropertyChanged("NameProperty");
                //PropertySetDefinition.ModelObject.OnPropertyChanged("CollectionPropertySet");
            }
        }

        public BasePropertySetDefinition PropertySetDefinition { get; }
        public IIfcPhysicalQuantity Property { get; }

        public string DataType
        {
            get
            {
                if (Property is IIfcQuantityArea quantityArea)
                {
                    return quantityArea.AreaValue.Value.GetType().Name;
                }
                else if (Property is IIfcQuantityCount quantityCount)
                {
                    return quantityCount.CountValue.Value.GetType().Name;
                }
                else if (Property is IIfcQuantityLength quantityLength)
                {
                    return quantityLength.LengthValue.Value.GetType().Name;
                }
                else if (Property is IIfcQuantityTime quantityTime)
                {
                    return quantityTime.TimeValue.Value.GetType().Name;
                }
                else if (Property is IIfcQuantityVolume quantityVolume)
                {
                    return quantityVolume.VolumeValue.Value.GetType().Name;
                }
                else if (Property is IIfcQuantityWeight quantityWeight)
                {
                    return quantityWeight.WeightValue.Value.GetType().Name;
                }

                return "Неизвестный тип";
            }
        }

        public string ValueString
        {
            get
            {
                if (Property is IIfcQuantityArea quantityArea)
                {
                    return Convert.ToString(quantityArea.AreaValue);
                }
                else if (Property is IIfcQuantityCount quantityCount)
                {
                    return Convert.ToString(quantityCount.CountValue);
                }
                else if (Property is IIfcQuantityLength quantityLength)
                {
                    return Convert.ToString(quantityLength.LengthValue);
                }
                else if (Property is IIfcQuantityTime quantityTime)
                {
                    return Convert.ToString(quantityTime.TimeValue);
                }
                else if (Property is IIfcQuantityVolume quantityVolume)
                {
                    return Convert.ToString(quantityVolume.VolumeValue);
                }
                else if (Property is IIfcQuantityWeight quantityWeight)
                {
                    return Convert.ToString(quantityWeight.WeightValue);
                }

                throw new ArgumentException("Exception ValueString");
            }
            set
            {
                ModelIFC.ChangeValue(new List<(Action<string>, string)> { (SetNewValue, value) });
                //OnPropertyChanged("Property");
                OnPropertyChanged("ValueString");
                OnPropertyChanged("DataType");
                //PropertySetDefinition.ModelObject.OnPropertyChanged("CollectionPropertySet");
            }
        }

        public abstract void SetNewValue(string stringValue);

        protected abstract IIfcValue GetPhysicalSimpleQuantityValue();

        protected abstract IIfcPhysicalSimpleQuantity SetNewPhysicalSimpleQuantityValue(string newValueString, IIfcPhysicalSimpleQuantity ifcPhisSimpQuantity);
    }

    public interface IPropertyModel<out T> : INotifyPropertyChanged
    {
        public T Property { get; }
        public string DataType { get; }
        public string ValueString { get; set; }
        public string NameProperty { get; set; }
        public BasePropertySetDefinition PropertySetDefinition { get; }
        public void SetNewValue(string stringValue);
    }
}