﻿using System.Collections.ObjectModel;
using Editor_IFC;
using RZDP_IFC_Viewer.IFC.Model;
using RZDP_IFC_Viewer.IFC.ModelItem;
using IFC_Viewer.IFC.Base;
using RZDP_IFC_Viewer.IFC.Model.ModelObjectPropertySet.Base;
using Xbim.Ifc2x3.Kernel;
using Xbim.Ifc2x3.PropertyResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Common;
using Xbim.Ifc2x3.MeasureResource;

namespace IFC_Viewer.IFC.Fabric
{
    public class EditorItemIFC2x3 : BaseEditorItem
    {
        private IfcObjectDefinition _ifcObjectDefinition;

        public EditorItemIFC2x3(ModelItemIFCObject modelItemIFCObject, ModelIFC modelIFC, IfcObjectDefinition IFCObjectDefinition) : base(modelItemIFCObject, modelIFC, IFCObjectDefinition)
        {
            this._ifcObjectDefinition = IFCObjectDefinition;
        }


        public override IEnumerable<IPersistEntity> DeletePropertySet(IIfcPropertySetDefinition ifcPropertySetDefinition)
        {
            if (_ifcObjectDefinition is IfcObject ifcObject)
            {
                IfcRelDefinesByProperties? rpRelDef =  ifcObject.IsDefinedByProperties.FirstOrDefault(prDef => prDef.RelatingPropertyDefinition == ifcPropertySetDefinition);

                if (rpRelDef != null)
                {
                    yield return rpRelDef;
                }

                yield return ifcPropertySetDefinition;
            }
        }

        #region Добавление ссылок

        /// <summary>
        /// Добавление ссылок
        /// </summary>
        /// <param name="modelReferenceSet"></param>
        public override bool AddReferenceToTheObject(List<BaseModelReferenceIFC> modelReferenceSet)
        {
            IfcPropertySet? ifcPropSetReference = FillCollectionPropertySet().
                                                Select(it => it.IFCPropertySetDefinition).
                                                OfType<IfcPropertySet>().
                                                FirstOrDefault(it => it.Name == "RZDP_Ссылки");

            if (ifcPropSetReference == null)
            {
                ifcPropSetReference = ModelIFC.IfcStore.Instances.New<IfcPropertySet>(prSet =>
                {
                    prSet.Name = "RZDP_Ссылки";
                });

                AddPropertySet(ifcPropSetReference);
            }

            foreach (BaseModelReferenceIFC modelReference in modelReferenceSet)
            {
                //Если в наборе уже есть такая ссылка то пропускаем цикл
                if ((ifcPropSetReference.HasProperties.
                                    OfType<IfcPropertyReferenceValue>().
                                    FirstOrDefault(it => it.PropertyReference == modelReference.GetReference())) != null)
                {
                    continue;
                }
                IfcPropertyReferenceValue pref = ModelIFC.IfcStore.Instances.New<IfcPropertyReferenceValue>(pr =>
                {
                    pr.Name = modelReference.NameReference;
                    pr.PropertyReference = modelReference.GetReference() as IfcObjectReferenceSelect;
                });

                ifcPropSetReference.HasProperties.Add(pref);

                //Добавляем в таблицу ссылку на текущий элемент
                modelReference.AddReferenceToTheElement(modelItemIFCObject);
            }

            ////Обновляем коллекцию характеристик
            //FillCollectionPropertySet();

            return modelReferenceSet.Count > 0;
        }

        #endregion Добавление ссылок

        #region Создать набор характеристик
        public override IIfcPropertySetDefinition CreateNewPropertySet(string namePropertySet = "Новый набор")
        {
            IfcPropertySet newPropertySet = ModelIFC.IfcStore.Model.Instances.New<IfcPropertySet>(prS =>
            { prS.Name = namePropertySet; });
            return AddPropertySet(newPropertySet);
        }

        public override IIfcPropertySetDefinition CreateNewPropertySet(string namePropertySet, List<(string, object)> collectionParameters)
        {
            IfcPropertySet newPropertySet = ModelIFC.IfcStore.Model.Instances.New<IfcPropertySet>(prS =>
            {
                prS.Name = namePropertySet;
                foreach (var parameter in collectionParameters)
                {
                    IfcPropertySingleValue ifcProp = ModelIFC.IfcStore.Model.Instances.New<IfcPropertySingleValue>(prop =>
                    {
                        prop.Name = parameter.Item1;
                        if (parameter.Item2 is IfcValue ifcValue)
                        {
                            prop.NominalValue = ifcValue;
                        }
                        else if (parameter.Item2 is string text)
                        {
                            prop.NominalValue = new IfcText(text);
                        }

                    });
                    prS.HasProperties.Add(ifcProp);
                }
            });

            return AddPropertySet(newPropertySet);
        }
        #endregion

        #region Добавить набор характеристик

        protected override IIfcPropertySetDefinition AddPropertySet(IIfcPropertySetDefinition iIfcPropertySet)
        {
            IfcPropertySetDefinition? ifcPropertySetDefinition = iIfcPropertySet as IfcPropertySetDefinition;

            if (ifcObjectDefinition is IfcObject ifcObject)
            {
                IfcRelDefinesByProperties ifcRelDefinesByProperties = ModelIFC.IfcStore.Instances.New<IfcRelDefinesByProperties>(relDef =>
                {
                    relDef.RelatedObjects.Add(ifcObject);
                    relDef.RelatingPropertyDefinition = ifcPropertySetDefinition;
                });
                return ifcPropertySetDefinition;
            }
            else
            {
                throw new ArgumentException($"Объект {ifcObjectDefinition.Name}, не является типом IfcObject");
            }
        }

        #endregion Добавить набор характеристик

        #region Заполнение характеристик элемента

        //public bool FillCollectionPropertySet(ObservableCollection<IIfcPropertySetDefinition> CollectionPropertySet)
        //{
        //    if (this.ifcObjectDefinition == null)
        //    {
        //        return false;
        //    }

        //    CollectionPropertySet.Clear();
        //    if (ifcObjectDefinition is IIfcObject obj)
        //    {
        //        var collectionProperty = obj.IsDefinedBy.Select(it => it.RelatingPropertyDefinition)
        //            .OfType<IfcPropertySetDefinition>();

        //        foreach (IfcPropertySetDefinition propSetIsObj in collectionProperty)
        //        {
        //            CollectionPropertySet.Add(propSetIsObj);
        //        }

        //        IEnumerable<IfcPropertySetDefinition> collectionTypeProperty = obj.IsTypedBy.SelectMany(it => it.RelatingType.HasPropertySets).Cast<IfcPropertySetDefinition>();

        //        if (collectionTypeProperty != null)
        //        {
        //            foreach (IfcPropertySetDefinition propSetIsType in collectionTypeProperty)
        //            {
        //                CollectionPropertySet.Add(propSetIsType);
        //            }
        //        }
        //    }
            //else if (ifcObjectDefinition is IIfcContext context)
            //{
            //    IEnumerable<IfcPropertySetDefinition> collectionProperty = context.IsDefinedBy.Select(it => it.RelatingPropertyDefinition).OfType<IfcPropertySetDefinition>();

            //    foreach (IfcPropertySetDefinition propSetIsObj in collectionProperty)
            //    {
            //        CollectionPropertySet.Add(propSetIsObj);
            //    }
            //}

        //    return true;
        //}




        #endregion Заполнение характеристик элемента
    }
}