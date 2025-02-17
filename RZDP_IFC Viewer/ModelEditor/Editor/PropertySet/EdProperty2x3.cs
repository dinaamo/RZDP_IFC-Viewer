using RZDP_IFC_Viewer.IFC.Model;
using RZDP_IFC_Viewer.IFC.ModelItem;
using RZDP_IFC_Viewer.IFC.Model.ModelObjectPropertySet.Base;
using Xbim.Ifc2x3.MeasureResource;
using Xbim.Ifc2x3.PropertyResource;
using Xbim.Ifc2x3.QuantityResource;
using Xbim.Ifc4.Interfaces;

namespace Editor_IFC
{
    public class EditorProperty2x3<T> : BaseEditorProperty<T>
    {
        public EditorProperty2x3(T Value, ModelIFC modelIFC, BasePropertySetDefinition propertySetDefinition) : base(Value, modelIFC, propertySetDefinition)
        { }

        public override void SetNewValue(string stringValue)
        {
            if (Property is IfcPropertySingleValue ifcPropertySingleValue)
            {
                SetNewValueForProperty(stringValue, ifcPropertySingleValue.NominalValue);
            }
        }

        public override object Value
        {
            get
            {
                if (Property is IfcPropertySingleValue ifcValue)
                {
                    return ifcValue.NominalValue == null ? string.Empty : ifcValue.NominalValue;
                }
                else if (Property is IfcPropertyReferenceValue ifcRefValue)
                {
                    return ifcRefValue.PropertyReference;
                }
                return string.Empty;
            }
        }

        protected override void SetNewValueForProperty<T>(string newValueString, T simpleValue)
        {
            newValueString = newValueString.Replace(',', '.');
            var ifcSingleValue = Property as IfcPropertySingleValue;

            if (simpleValue is null)
            {
                ifcSingleValue.NominalValue = new IfcText(newValueString);
                return;
            }

            try
            {
                if (simpleValue is IfcReal)
                {
                    ifcSingleValue.NominalValue = new IfcReal(newValueString);
                }
                if (simpleValue is IfcReal)
                {
                    ifcSingleValue.NominalValue = new IfcReal(newValueString);
                }
                else if (simpleValue is IfcInteger)
                {
                    ifcSingleValue.NominalValue = new IfcInteger(newValueString);
                }
                else if (simpleValue is IfcBoolean)
                {
                    ifcSingleValue.NominalValue = new IfcBoolean(newValueString);
                }
                else if (simpleValue is IfcLogical)
                {
                    ifcSingleValue.NominalValue = new IfcLogical(newValueString);
                }
                else if (simpleValue is IfcIdentifier)
                {
                    ifcSingleValue.NominalValue = new IfcIdentifier(newValueString);
                }
                else if (simpleValue is IfcLabel)
                {
                    ifcSingleValue.NominalValue = new IfcLabel(newValueString);
                }
                else if (simpleValue is IfcTimeStamp)
                {
                    ifcSingleValue.NominalValue = new IfcTimeStamp(newValueString);
                }
                else if (simpleValue is IfcAmountOfSubstanceMeasure)
                {
                    ifcSingleValue.NominalValue = new IfcAmountOfSubstanceMeasure(newValueString);
                }
                else if (simpleValue is IfcAreaMeasure)
                {
                    ifcSingleValue.NominalValue = new IfcAreaMeasure(newValueString);
                }
                else if (simpleValue is IfcContextDependentMeasure)
                {
                    ifcSingleValue.NominalValue = new IfcContextDependentMeasure(newValueString);
                }
                else if (simpleValue is IfcCountMeasure)
                {
                    ifcSingleValue.NominalValue = new IfcCountMeasure(newValueString);
                }
                else if (simpleValue is IfcDescriptiveMeasure)
                {
                    ifcSingleValue.NominalValue = new IfcDescriptiveMeasure(newValueString);
                }
                else if (simpleValue is IfcElectricCurrentMeasure)
                {
                    ifcSingleValue.NominalValue = new IfcElectricCurrentMeasure(newValueString);
                }
                else if (simpleValue is IfcLengthMeasure)
                {
                    ifcSingleValue.NominalValue = new IfcLengthMeasure(newValueString);
                }
                else if (simpleValue is IfcLuminousIntensityMeasure)
                {
                    ifcSingleValue.NominalValue = new IfcLuminousIntensityMeasure(newValueString);
                }
                else if (simpleValue is IfcMassMeasure)
                {
                    ifcSingleValue.NominalValue = new IfcMassMeasure(newValueString);
                }
                else if (simpleValue is IfcNormalisedRatioMeasure)
                {
                    ifcSingleValue.NominalValue = new IfcNormalisedRatioMeasure(newValueString);
                }
                else if (simpleValue is IfcNumericMeasure)
                {
                    ifcSingleValue.NominalValue = new IfcNumericMeasure(newValueString);
                }
                else if (simpleValue is IfcParameterValue)
                {
                    ifcSingleValue.NominalValue = new IfcParameterValue(newValueString);
                }
                else if (simpleValue is IfcPlaneAngleMeasure)
                {
                    ifcSingleValue.NominalValue = new IfcPlaneAngleMeasure(newValueString);
                }
                else if (simpleValue is IfcPositiveLengthMeasure)
                {
                    ifcSingleValue.NominalValue = new IfcPositiveLengthMeasure(newValueString);
                }
                else if (simpleValue is IfcPositivePlaneAngleMeasure)
                {
                    ifcSingleValue.NominalValue = new IfcPositivePlaneAngleMeasure(newValueString);
                }
                else if (simpleValue is IfcPositiveRatioMeasure)
                {
                    ifcSingleValue.NominalValue = new IfcPositiveRatioMeasure(newValueString);
                }
                else if (simpleValue is IfcRatioMeasure)
                {
                    ifcSingleValue.NominalValue = new IfcRatioMeasure(newValueString);
                }
                else if (simpleValue is IfcSolidAngleMeasure)
                {
                    ifcSingleValue.NominalValue = new IfcSolidAngleMeasure(newValueString);
                }
                else if (simpleValue is IfcThermodynamicTemperatureMeasure)
                {
                    ifcSingleValue.NominalValue = new IfcThermodynamicTemperatureMeasure(newValueString);
                }
                else if (simpleValue is IfcTimeMeasure)
                {
                    ifcSingleValue.NominalValue = new IfcTimeMeasure(newValueString);
                }
                else if (simpleValue is IfcVolumeMeasure)
                {
                    ifcSingleValue.NominalValue = new IfcVolumeMeasure(newValueString);
                }
                else if (simpleValue is IfcText)
                {
                    ifcSingleValue.NominalValue = new IfcText(newValueString);
                }
            }
            catch (FormatException)
            {
            }
        }
    }

    public class EditorQuantity2x3 : BaseEditorQuantity<IIfcPhysicalQuantity>
    {
        public EditorQuantity2x3(IfcPhysicalQuantity ifcPhysicalQuantity, ModelIFC modelIFC, BasePropertySetDefinition propertySetDefinition) : base(ifcPhysicalQuantity, modelIFC, propertySetDefinition)
        { }

        public override object Value
        {
            get
            {
                if (Property is IfcQuantityArea quantityArea)
                {
                    return quantityArea.AreaValue;
                }
                else if (Property is IfcQuantityCount quantityCount)
                {
                    return quantityCount.CountValue;
                }
                else if (Property is IfcQuantityLength quantityLength)
                {
                    return quantityLength.LengthValue;
                }
                else if (Property is IfcQuantityTime quantityTime)
                {
                    return quantityTime.TimeValue;
                }
                else if (Property is IfcQuantityVolume quantityVolume)
                {
                    return quantityVolume.VolumeValue;
                }
                else if (Property is IfcQuantityWeight quantityWeight)
                {
                    return quantityWeight.WeightValue;
                }

                throw new ArgumentException("Exception ValueString");
            }
        }


        protected override IIfcValue GetPhysicalSimpleQuantityValue()
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