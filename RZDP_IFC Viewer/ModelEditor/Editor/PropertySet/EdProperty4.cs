using IFC_Table_View.IFC.Model;
using IFC_Table_View.IFC.ModelItem;
using RZDP_IFC_Viewer.IFC.Model.ModelObjectPropertySet.Base;
using Xbim.Ifc4.DateTimeResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.QuantityResource;

namespace Editor_IFC
{
    public class EditorProperty4<T> : BaseEditorProperty<T>
    {
        public EditorProperty4(T Value, ModelIFC modelIFC, BasePropertySetDefinition propertySetDefinition) : base(Value, modelIFC, propertySetDefinition)
        { }

        public override void SetNewValue(string stringValue)
        {
            if (Property is IIfcPropertySingleValue ifcPropertySingleValue)
            {
                SetNewValueForProperty(stringValue, ifcPropertySingleValue.NominalValue);
            }
        }

        protected override void SetNewValueForProperty<T>(string newValueString, T simpleValue)
        {
            IfcValue newValue = null;

            if (Property is IIfcPropertySingleValue ifcSingleValue)
            {
                try
                {
                    if (simpleValue is IfcReal)
                    {
                        newValue = new IfcReal(newValueString);
                    }
                    else if (simpleValue is IfcInteger)
                    {
                        newValue = new IfcInteger(newValueString);
                    }
                    else if (simpleValue is IfcBoolean)
                    {
                        newValue = new IfcBoolean(newValueString);
                    }
                    else if (simpleValue is IfcLogical)
                    {
                        newValue = new IfcLogical(newValueString);
                    }
                    else if (simpleValue is IfcIdentifier)
                    {
                        newValue = new IfcIdentifier(newValueString);
                    }
                    else if (simpleValue is IfcLabel)
                    {
                        newValue = new IfcLabel(newValueString);
                    }
                    else if (simpleValue is IfcTimeStamp)
                    {
                        newValue = new IfcTimeStamp(newValueString);
                    }
                    else if (simpleValue is IfcAmountOfSubstanceMeasure)
                    {
                        newValue = new IfcAmountOfSubstanceMeasure(newValueString);
                    }
                    else if (simpleValue is IfcAreaMeasure)
                    {
                        newValue = new IfcAreaMeasure(newValueString);
                    }
                    else if (simpleValue is IfcContextDependentMeasure)
                    {
                        newValue = new IfcContextDependentMeasure(newValueString);
                    }
                    else if (simpleValue is IfcCountMeasure)
                    {
                        newValue = new IfcCountMeasure(newValueString);
                    }
                    else if (simpleValue is IfcDescriptiveMeasure)
                    {
                        newValue = new IfcDescriptiveMeasure(newValueString);
                    }
                    else if (simpleValue is IfcElectricCurrentMeasure)
                    {
                        newValue = new IfcElectricCurrentMeasure(newValueString);
                    }
                    else if (simpleValue is IfcLengthMeasure)
                    {
                        newValue = new IfcLengthMeasure(newValueString);
                    }
                    else if (simpleValue is IfcLuminousIntensityMeasure)
                    {
                        newValue = new IfcLuminousIntensityMeasure(newValueString);
                    }
                    else if (simpleValue is IfcMassMeasure)
                    {
                        newValue = new IfcMassMeasure(newValueString);
                    }
                    else if (simpleValue is IfcNormalisedRatioMeasure)
                    {
                        newValue = new IfcNormalisedRatioMeasure(newValueString);
                    }
                    else if (simpleValue is IfcNumericMeasure)
                    {
                        newValue = new IfcNumericMeasure(newValueString);
                    }
                    else if (simpleValue is IfcParameterValue)
                    {
                        newValue = new IfcParameterValue(newValueString);
                    }
                    else if (simpleValue is IfcPlaneAngleMeasure)
                    {
                        newValue = new IfcPlaneAngleMeasure(newValueString);
                    }
                    else if (simpleValue is IfcPositiveLengthMeasure)
                    {
                        newValue = new IfcPositiveLengthMeasure(newValueString);
                    }
                    else if (simpleValue is IfcPositivePlaneAngleMeasure)
                    {
                        newValue = new IfcPositivePlaneAngleMeasure(newValueString);
                    }
                    else if (simpleValue is IfcPositiveRatioMeasure)
                    {
                        newValue = new IfcPositiveRatioMeasure(newValueString);
                    }
                    else if (simpleValue is IfcRatioMeasure)
                    {
                        newValue = new IfcRatioMeasure(newValueString);
                    }
                    else if (simpleValue is IfcSolidAngleMeasure)
                    {
                        newValue = new IfcSolidAngleMeasure(newValueString);
                    }
                    else if (simpleValue is IfcThermodynamicTemperatureMeasure)
                    {
                        newValue = new IfcThermodynamicTemperatureMeasure(newValueString);
                    }
                    else if (simpleValue is IfcTimeMeasure)
                    {
                        newValue = new IfcTimeMeasure(newValueString);
                    }
                    else if (simpleValue is IfcVolumeMeasure)
                    {
                        newValue = new IfcVolumeMeasure(newValueString);
                    }
                    else if (simpleValue is IfcText)
                    {
                        newValue = new IfcText(newValueString);
                    }
                }
                catch (FormatException)
                {
                    //ifcSingleValue.NominalValue = new IfcText(newValueString);
                }

                if (newValue is not null)
                {
                    ifcSingleValue.NominalValue = newValue;
                }
                else
                {
                    ifcSingleValue.NominalValue = new IfcText(newValueString);
                }
            }
        }
    }

    public class EditorQuantity4 : BaseEditorQuantity<IIfcPhysicalQuantity>
    {
        public EditorQuantity4(IIfcPhysicalQuantity ifcPhysicalQuantity, ModelIFC modelIFC, BasePropertySetDefinition propertySetDefinition) : base(ifcPhysicalQuantity, modelIFC, propertySetDefinition)
        { }

        protected override IfcValue GetPhysicalSimpleQuantityValue()
        {
            if (Property is IIfcQuantityArea quantityArea)
            {
                return quantityArea.AreaValue;
            }
            else if (Property is IIfcQuantityCount quantityCount)
            {
                return quantityCount.CountValue;
            }
            else if (Property is IIfcQuantityLength quantityLength)
            {
                return quantityLength.LengthValue;
            }
            else if (Property is IIfcQuantityTime quantityTime)
            {
                return quantityTime.TimeValue;
            }
            else if (Property is IIfcQuantityVolume quantityVolume)
            {
                return quantityVolume.VolumeValue;
            }
            else if (Property is IIfcQuantityWeight quantityWeight)
            {
                return quantityWeight.WeightValue;
            }
            else
            {
                throw new ArgumentException("Ошибка GetPhysicalSimpleQuantityValue");
            }
        }

        protected override IIfcPhysicalSimpleQuantity SetNewPhysicalSimpleQuantityValue(string newValueString, IIfcPhysicalSimpleQuantity ifcPhisSimpQuantity)
        {
            try
            {
                newValueString = newValueString.Replace(',', '.');

                if (ifcPhisSimpQuantity is IfcQuantityArea ifcQuantityArea)
                {
                    ifcQuantityArea.AreaValue = new IfcAreaMeasure(newValueString);
                }
                else if (ifcPhisSimpQuantity is IfcQuantityCount ifcQuantityCount)
                {
                    ifcQuantityCount.CountValue = new IfcCountMeasure();
                }
                else if (ifcPhisSimpQuantity is IfcQuantityLength ifcQuantityLength)
                {
                    ifcQuantityLength.LengthValue = new IfcLengthMeasure(newValueString);
                }
                else if (ifcPhisSimpQuantity is IfcQuantityTime ifcQuantityTime)
                {
                    ifcQuantityTime.TimeValue = new IfcTimeMeasure(newValueString);
                }
                else if (ifcPhisSimpQuantity is IfcQuantityVolume ifcQuantityVolume)
                {
                    ifcQuantityVolume.VolumeValue = new IfcVolumeMeasure(newValueString);
                }
                else if (ifcPhisSimpQuantity is IfcQuantityWeight ifcQuantityWeight)
                {
                    ifcQuantityWeight.WeightValue = new IfcMassMeasure(newValueString);
                }
            }
            catch (FormatException)
            {
            }

            return ifcPhisSimpQuantity;
        }

        public override void SetNewValue(string stringValue)
        {
            if (Property is IIfcPhysicalSimpleQuantity ifcPhysicalSimpleQuantity)
            {
                SetNewPhysicalSimpleQuantityValue(stringValue, ifcPhysicalSimpleQuantity);
            }
        }
    }
}