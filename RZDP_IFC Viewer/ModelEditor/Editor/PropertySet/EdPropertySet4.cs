using System.Collections.ObjectModel;
using System.Windows;
using IFC_Table_View.IFC.Model;
using IFC_Table_View.IFC.ModelItem;
using RZDP_IFC_Viewer.IFC.Model.ModelObjectPropertySet.Base;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.ProductExtension;
using Xbim.Ifc4.PropertyResource;
using Xbim.Ifc4.QuantityResource;

namespace Editor_IFC
{
    public class EdPropertySet4 : BasePropertySetDefinition
    {
        private IfcPropertySet ifcPropertySet;

        public EdPropertySet4(IIfcObjectDefinition ifcObjectDefinition, IfcPropertySet ifcPropertySet, ModelIFC modelIFC, ModelItemIFCObject modelItem) : base(ifcObjectDefinition, ifcPropertySet, modelIFC, modelItem)
        {
            this.ifcPropertySet = ifcPropertySet;
        }

        public override void AddProperty(string nameProperty, string valueProperty)
        {
            IfcPropertySingleValue ifcProp = ModelIFC.IfcStore.Model.Instances.New<IfcPropertySingleValue>(prop =>
            {
                prop.Name = nameProperty;
                prop.NominalValue = new IfcText(valueProperty);
            });

            ifcPropertySet.HasProperties.Add(ifcProp);
            OnPropertyChanged("PropertyCollection");
        }

        public override void CreateNewProperty()
        {
            IfcPropertySingleValue ifcProp = ModelIFC.IfcStore.Model.Instances.New<IfcPropertySingleValue>(prop =>
            {
                prop.Name = "Новый параметр";
                prop.NominalValue = new IfcText("Новое значение");
            });

            ifcPropertySet.HasProperties.Add(ifcProp);
            OnPropertyChanged("PropertyCollection");
        }

        public override IfcPropertySet GetCopyPropertySet()
        {
            IfcPropertySet newPropertySet = ModelIFC.IfcStore.Model.Instances.New<IfcPropertySet>(prS =>
            {
                prS.Name = NamePropertySet;

                foreach (var Property in PropertyCollection.OfType<IPropertyModel<IfcPropertySingleValue>>())
                {
                    IfcPropertySingleValue ifcProp = ModelIFC.IfcStore.Model.Instances.New<IfcPropertySingleValue>(prop =>
                    {
                        prop.Name = Property.NameProperty;
                        prop.NominalValue = Property.Property.NominalValue;
                    });

                    prS.HasProperties.Add(ifcProp);
                }
            });

            return newPropertySet;
        }

        protected override IEnumerable<IPropertyModel<IIfcResourceObjectSelect>> FillCollectionProperty()
        {
            //ObservableCollection<IPropertyModel<IIfcResourceObjectSelect>> CollectionProperty = new ObservableCollection<IPropertyModel<IIfcResourceObjectSelect>>();
            foreach (IIfcProperty property in ifcPropertySet.HasProperties.Where(it => it is IIfcPropertySingleValue || it is IIfcPropertyReferenceValue))
            {
                //CollectionProperty.Add(
                    yield return new EditorProperty4<IIfcProperty>(property, ModelIFC, this);
            }
            //return CollectionProperty;
        }
    }

    public class EdElementQuantity4 : BasePropertySetDefinition
    {
        private IfcElementQuantity ElementQuantity;

        public EdElementQuantity4(IIfcObjectDefinition ifcObject, IfcElementQuantity ElementQuantity, ModelIFC modelIFC, ModelItemIFCObject modelItem) : base(ifcObject, ElementQuantity, modelIFC, modelItem)
        {
            this.ElementQuantity = ElementQuantity;
            FillCollectionProperty();
        }

        public override void AddProperty(string nameProperty, string valueProperty)
        {
            double value;
            double.TryParse(valueProperty, out value);
            IfcQuantityLength ifcProp = ModelIFC.IfcStore.Model.Instances.New<IfcQuantityLength>(prop =>
            {
                prop.Name = nameProperty;
                prop.LengthValue = new IfcLengthMeasure(value);
            });

            ElementQuantity.Quantities.Add(ifcProp);
            OnPropertyChanged("PropertyCollection");
        }

        public override void CreateNewProperty()
        {
            IfcQuantityLength ifcProp = ModelIFC.IfcStore.Model.Instances.New<IfcQuantityLength>(prop =>
            {
                prop.Name = "Новый параметр";
                prop.LengthValue = new IfcLengthMeasure(0);
            });

            ElementQuantity.Quantities.Add(ifcProp);
            OnPropertyChanged("PropertyCollection");
        }

        //public override bool DeletePropertyModel(IPropertyModel<IIfcResourceObjectSelect> propertyModel)
        //{
        //    ModelObject.ModelIFC.DeleteIFCEntity(propertyModel.Property);
        //    return true;
        //}

        protected override IEnumerable<IPropertyModel<IIfcResourceObjectSelect>> FillCollectionProperty()
        {
            //ObservableCollection<IPropertyModel<IIfcResourceObjectSelect>> CollectionProperty = new ObservableCollection<IPropertyModel<IIfcResourceObjectSelect>>();

            foreach (IIfcPhysicalQuantity quantity in ElementQuantity.Quantities.OfType<IIfcPhysicalQuantity>())
            {
                if (quantity is IfcQuantityArea quantityArea)
                {
                    //CollectionProperty.Add(
                        yield return new EditorQuantity4(quantityArea, ModelIFC, this);
                }
                else if (quantity is IfcQuantityCount quantityCount)
                {
                    //CollectionProperty.Add(
                        yield return new EditorQuantity4(quantityCount, ModelIFC, this);
                }
                else if (quantity is IfcQuantityLength quantityLength)
                {
                    //CollectionProperty.Add(
                        yield return new EditorQuantity4(quantityLength, ModelIFC, this);
                }
                else if (quantity is IfcQuantityTime quantityTime)
                {
                    //CollectionProperty.Add(
                        yield return new EditorQuantity4(quantityTime, ModelIFC, this);
                }
                else if (quantity is IfcQuantityVolume quantityVolume)
                {
                   //CollectionProperty.Add(
                       yield return new EditorQuantity4(quantityVolume, ModelIFC, this);
                }
                else if (quantity is IfcQuantityWeight quantityWeight)
                {
                    //CollectionProperty.Add(
                        yield return new EditorQuantity4(quantityWeight, ModelIFC, this);
                }
            }
            //return CollectionProperty;
        }

        public override IfcElementQuantity GetCopyPropertySet()
        {
            IfcElementQuantity newElementQuantity = ModelIFC.IfcStore.Instances.New<IfcElementQuantity>(prS =>
            {
                prS.Name = NamePropertySet;

                foreach (IPropertyModel<IIfcPhysicalQuantity> property in PropertyCollection.OfType<IPropertyModel<IIfcPhysicalQuantity>>())
                {
                    if (property.Property is IfcQuantityArea quantityArea)
                    {
                        IfcPhysicalSimpleQuantity ifcPhysicalSimpleQuantity = ModelIFC.IfcStore.Model.Instances.
                        New<IfcQuantityArea>(physical =>
                        {
                            physical.Name = property.NameProperty;
                            physical.AreaValue = quantityArea.AreaValue;
                        });
                        prS.Quantities.Add(ifcPhysicalSimpleQuantity);
                    }
                    else if (property.Property is IfcQuantityCount quantityCount)
                    {
                        IfcPhysicalSimpleQuantity ifcPhysicalSimpleQuantity = ModelIFC.IfcStore.Model.Instances.
                        New<IfcQuantityCount>(physical =>
                        {
                            physical.Name = property.NameProperty;
                            physical.CountValue = quantityCount.CountValue;
                        });
                        prS.Quantities.Add(ifcPhysicalSimpleQuantity);
                    }
                    else if (property.Property is IfcQuantityLength quantityLength)
                    {
                        IfcPhysicalSimpleQuantity ifcPhysicalSimpleQuantity = ModelIFC.IfcStore.Model.Instances.
                        New<IfcQuantityLength>(physical =>
                        {
                            physical.Name = property.NameProperty;
                            physical.LengthValue = quantityLength.LengthValue;
                        });
                        prS.Quantities.Add(ifcPhysicalSimpleQuantity);
                    }
                    else if (property.Property is IfcQuantityTime quantityTime)
                    {
                        IfcPhysicalSimpleQuantity ifcPhysicalSimpleQuantity = ModelIFC.IfcStore.Model.Instances.
                        New<IfcQuantityTime>(physical =>
                        {
                            physical.Name = property.NameProperty;
                            physical.TimeValue = quantityTime.TimeValue;
                        });
                        prS.Quantities.Add(ifcPhysicalSimpleQuantity);
                    }
                    else if (property.Property is IfcQuantityVolume quantityVolume)
                    {
                        IfcPhysicalSimpleQuantity ifcPhysicalSimpleQuantity = ModelIFC.IfcStore.Model.Instances.
                        New<IfcQuantityVolume>(physical =>
                        {
                            physical.Name = property.NameProperty;
                            physical.VolumeValue = quantityVolume.VolumeValue;
                        });
                        prS.Quantities.Add(ifcPhysicalSimpleQuantity);
                    }
                    else if (property.Property is IfcQuantityWeight quantityWeight)
                    {
                        IfcPhysicalSimpleQuantity ifcPhysicalSimpleQuantity = ModelIFC.IfcStore.Model.Instances.
                        New<IfcQuantityWeight>(physical =>
                        {
                            physical.Name = property.NameProperty;
                            physical.WeightValue = quantityWeight.WeightValue;
                        });
                        prS.Quantities.Add(ifcPhysicalSimpleQuantity);
                    }
                }
            });

            return newElementQuantity;
        }
    }
}