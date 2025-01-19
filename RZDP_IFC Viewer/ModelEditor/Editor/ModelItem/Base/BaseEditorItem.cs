using System.Collections.ObjectModel;
using Editor_IFC;
using IFC_Table_View.IFC.Model;
using IFC_Table_View.IFC.ModelItem;
using IFC_Viewer.IFC.Fabric;
using IFC_Viewer.IFC.ModelItem;
using RZDP_IFC_Viewer.IFC.Editor.Base;
using Xbim.Ifc4.Interfaces;

namespace IFC_Viewer.IFC.Base
{
    public abstract class BaseEditorItem : BaseModel
    {
        protected IIfcObjectDefinition ifcObjectDefinition;

        //protected ObservableCollection<BasePropertySetDefinition> CollectionPropertySet;

        protected ModelItemIFCObject modelItemIFCObject;

        public BaseEditorItem(ModelItemIFCObject modelItemIFCObject, ModelIFC modelIFC, IIfcObjectDefinition IFCObjectDefinition) : base(modelIFC)
        {
            this.ifcObjectDefinition = IFCObjectDefinition;
            this.modelItemIFCObject = modelItemIFCObject;
        }

        #region Методы

        public abstract bool AddReferenceToTheObject(List<BaseModelReferenceIFC> modelReferenceSet);

        /// <summary>
        /// Удаление ссылок на документы или таблицы
        /// </summary>
        /// <param name="CollectionModelReferenceToDelete">Коллекция таблиц или ссылок на документы для удаления</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public int DeleteReferenceToTheObject(IEnumerable<BaseModelReferenceIFC> CollectionModelReferenceToDelete)
        {
            //Из коллекции наборов характеристик получаем наборы в которых есть ссылки
            List<IIfcPropertySet> PropSetReferenceCollection = FillCollectionPropertySet().
                                                Select(it => it.IFCPropertySetDefinition).
                                                OfType<IIfcPropertySet>().
                                                Where(it => it.HasProperties.Any(pr => pr is IIfcPropertyReferenceValue)).ToList();

            //Если таких не нашли то кидаем исключение
            if (PropSetReferenceCollection.Count() == 0)
            {
                throw new ArgumentNullException("Не найдены наборы с ссылками");
            }

           
            //Начинаем удалять ссылки
            foreach (BaseModelReferenceIFC modelReference in CollectionModelReferenceToDelete)
            {
                foreach (IIfcPropertySet ifcPropertySet in PropSetReferenceCollection.ToArray())
                {
                    //Ищем характеристику 
                    IIfcPropertyReferenceValue? propertyToDelete = ifcPropertySet.HasProperties
                                                                                    .OfType<IIfcPropertyReferenceValue>()
                                                                                    .FirstOrDefault(it => it.PropertyReference == modelReference.GetReference());

                    if (propertyToDelete is null)
                    { continue; }

                    //Если нашли
                    //Удаляем ссылку на modelObject 
                    modelReference.DeleteReferenceToTheElement(modelItemIFCObject);

                    //Удаляем из набора характеристик
                    ifcPropertySet.HasProperties.Remove(propertyToDelete);
                    //ModelIFC.DeleteIFCEntity(propertyToDelete);

                    //Если набор пустой то его удаляем 
                    if (ifcPropertySet.HasProperties.Count == 0)
                    {
                        PropSetReferenceCollection.Remove(ifcPropertySet);
                        ModelIFC.DeleteIFCEntity(ifcPropertySet);
                    }
                }
            }

            ///
            return PropSetReferenceCollection.Count();
            
        }

        protected abstract void AddPropertySet(IIfcPropertySet iIfcPropertySet);



        //public void AddReferenceToTheObjectReferenceToLoad(ObservableCollection<BaseModelReferenceIFC> ReferenceElementSet, ModelItemIFCObject modelItemIFCObject)
        //{
        //    IEnumerable<IPropertyModel<IIfcPropertyReferenceValue>> propertyReferenceSet =  FillCollectionPropertySet().
        //                                                SelectMany(it => it.PropertyCollection).
        //                                                OfType<IPropertyModel<IIfcPropertyReferenceValue>>();

        //    foreach (IPropertyModel<IIfcPropertyReferenceValue> propertyReference in propertyReferenceSet)
        //    {
        //        foreach (BaseModelReferenceIFC tableItem in ReferenceElementSet)
        //        {
        //            if (propertyReference.Property.Equals(tableItem.GetReference()))
        //            {
        //                tableItem.AddReferenceToTheElement(modelItemIFCObject);
        //            }
        //        }
        //    }
        //}

        #region Заполнение характеристик элемента

        public IEnumerable<BasePropertySetDefinition> FillCollectionPropertySet()
        {
            if (this.ifcObjectDefinition is null)
            {
                throw new ArgumentNullException("ifcObjectDefinition is null. Property set filling error");
            }

            if (ifcObjectDefinition is IIfcObject ifcObject)
            {
                var collectionProperty = ifcObject.IsDefinedBy.Select(it => it.RelatingPropertyDefinition)
                    .Where(pr => pr is IIfcPropertySet || pr is IIfcElementQuantity);

                foreach (IIfcPropertySetDefinition ifcPropSetIsObj in collectionProperty)
                {
                    yield return BasePropertySetDefinition.CreateEditorPropertySet(ifcObject, ifcPropSetIsObj, ModelIFC, modelItemIFCObject);
                }

                IEnumerable<IIfcPropertySetDefinition> collectionTypeProperty = ifcObject.IsTypedBy.SelectMany(it => it.RelatingType.HasPropertySets)
                    .Where(pr => pr is IIfcPropertySet || pr is IIfcElementQuantity);

                if (collectionTypeProperty != null)
                {
                    foreach (IIfcPropertySetDefinition propSetIsType in collectionTypeProperty)
                    {
                        yield return BasePropertySetDefinition.CreateEditorPropertySet(ifcObject, propSetIsType, ModelIFC, modelItemIFCObject);
                    }
                }
            }
            else if (ifcObjectDefinition is IIfcContext ifcContext)
            {
                IEnumerable<IIfcPropertySetDefinition> collectionProperty = ifcContext.IsDefinedBy.Select(it => it.RelatingPropertyDefinition).OfType<IIfcPropertySetDefinition>();

                foreach (IIfcPropertySetDefinition propSetIsObj in collectionProperty)
                {
                    yield return BasePropertySetDefinition.CreateEditorPropertySet(ifcContext, propSetIsObj, ModelIFC, modelItemIFCObject);
                }
            }

        }

        #endregion Заполнение характеристик элемента

        #region Заполнение свойств элемента

        public Dictionary<string, HashSet<object>> GetPropertyObject()
        {
            Dictionary<string, HashSet<object>> PropertyElement = new Dictionary<string, HashSet<object>>();
            if (this.ifcObjectDefinition == null)
            {
                return PropertyElement;
            }

            //Материал
            if (ifcObjectDefinition is IIfcElement IFCElement)
            {
                IIfcMaterialSelect ifcMaterialSelect = IFCElement.Material;

                if (ifcMaterialSelect != null)
                {
                    if (ifcMaterialSelect is IIfcMaterial materialSingle)
                    {
                        PropertyElement.Add("Материал", new HashSet<object>() { materialSingle.Name });
                    }
                    else if (ifcMaterialSelect is IIfcMaterialLayer materiaLayer)
                    {
                        PropertyElement.Add("Материал", new HashSet<object>() { materiaLayer.Material.Name });
                    }
                    else if (ifcMaterialSelect is IIfcMaterialLayerSet materiaLayerSet)
                    {
                        HashSet<object> materialSet = new HashSet<object>();
                        foreach (IIfcMaterial material in materiaLayerSet.MaterialLayers.Select(it => it.Material))
                        {
                            materialSet.Add(material.Name);
                        }
                        PropertyElement.Add("Материалы", materialSet);
                    }
                    else if (ifcMaterialSelect is IIfcMaterialProfile materiaProfile)
                    {
                        PropertyElement.Add("Материал", new HashSet<object>() { materiaProfile.Material.Name });
                    }
                    else if (ifcMaterialSelect is IIfcMaterialProfileSet materiaProfileSet)
                    {
                        HashSet<object> materialSet = new HashSet<object>();
                        foreach (IIfcMaterial material in materiaProfileSet.MaterialProfiles.Select(it => it.Material))
                        {
                            materialSet.Add(material.Name);
                        }
                        PropertyElement.Add("Материалы", materialSet);
                    }
                    else if (ifcMaterialSelect is IIfcMaterialConstituent materiaConstituen)
                    {
                        PropertyElement.Add("Материал", new HashSet<object>() { materiaConstituen.Material.Name });
                    }
                    else if (ifcMaterialSelect is IIfcMaterialConstituentSet materiaConstituentSet)
                    {
                        HashSet<object> materialSet = new HashSet<object>();
                        foreach (IIfcMaterial material in materiaConstituentSet.MaterialConstituents.Select(it => it.Material))
                        {
                            materialSet.Add(material.Name);
                        }
                        PropertyElement.Add("Материалы", materialSet);
                    }
                    else if (ifcMaterialSelect is IIfcMaterialLayerSetUsage materiaLayerSetUsage)
                    {
                        HashSet<object> materialSet = new HashSet<object>();
                        foreach (IIfcMaterial material in materiaLayerSetUsage.ForLayerSet.MaterialLayers.Select(it => it.Material))
                        {
                            materialSet.Add(material.Name);
                        }
                        PropertyElement.Add("Материалы", materialSet);
                    }
                    else if (ifcMaterialSelect is IIfcMaterialProfileSetUsage materiaProfileSetUsag)
                    {
                        HashSet<object> materialSet = new HashSet<object>();
                        foreach (IIfcMaterial material in materiaProfileSetUsag.ForProfileSet.MaterialProfiles.Select(it => it.Material))
                        {
                            materialSet.Add(material.Name);
                        }
                        PropertyElement.Add("Материалы", materialSet);
                    }
                    else if (ifcMaterialSelect is IIfcMaterialList materialList)
                    {
                        HashSet<object> materialListValue = new HashSet<object>();

                        foreach (IIfcMaterial material in materialList.Materials)
                        {
                            materialListValue.Add(material.Name);
                        }
                        PropertyElement.Add("Материалы", materialListValue);
                    }
                }
            }

            if (ifcObjectDefinition.Description != string.Empty)
            {
                PropertyElement.Add("Описание", new HashSet<object>() { ifcObjectDefinition.Description });
            }

            //Вложенные объекты
            HashSet<object> listobjectIsNestedBy = new HashSet<object>();
            foreach (IIfcRelNests relNest in ifcObjectDefinition.IsNestedBy)
            {
                foreach (IIfcObjectDefinition obj in relNest.RelatedObjects)
                {
                    if (obj == ifcObjectDefinition)
                    {
                        continue;
                    }
                    else
                    {
                        listobjectIsNestedBy.Add(
                            $"Наименование связи: {relNest.Name}\n" +
                            $"Описание связи: {relNest.Description}\n" +
                            $"Наименование элемента: {obj.Name}\n" +
                            $"Класс IFC: {obj.GetType().Name}\n" +
                            $"GUID: {obj.GlobalId}");
                    }
                }
            }
            if (listobjectIsNestedBy.Count > 0)
            {
                PropertyElement.Add("Вложенные объекты (IsNestedBy)", listobjectIsNestedBy);
            }

            //Разлагается на объекты
            HashSet<object> listobjectIsDecomposedBy = new HashSet<object>();
            foreach (IIfcRelAggregates relDecomp in ifcObjectDefinition.IsDecomposedBy)
            {
                foreach (IIfcObjectDefinition obj in relDecomp.RelatedObjects)
                {
                    if (obj == ifcObjectDefinition)
                    {
                        continue;
                    }
                    else
                    {
                        listobjectIsDecomposedBy.Add(
                            $"Наименование связи: {relDecomp.Name}\n" +
                            $"Описание связи: {relDecomp.Description}\n" +
                            $"Наименование элемента: {obj.Name}\n" +
                            $"Класс IFC: {obj.GetType().Name}\n" +
                            $"GUID: {obj.GlobalId}");
                    }
                }
            }
            if (listobjectIsDecomposedBy.Count > 0)
            {
                PropertyElement.Add("Раскладывается на объекты (IsDecomposedBy)", listobjectIsDecomposedBy);
            }

            if (ifcObjectDefinition is IIfcObject IFCObject)
            {
                //Тип объекта
                if (IFCObject.ObjectType != string.Empty)
                {
                    PropertyElement.Add("Тип", new HashSet<object>() { IFCObject.ObjectType });
                }

                //Связанные объекты
                HashSet<object> listObjectIsDefinedBy = new HashSet<object>();
                foreach (IIfcRelDefinesByProperties relDef in IFCObject.IsDefinedBy)
                {
                    foreach (IIfcObjectDefinition obj in relDef.RelatedObjects)
                    {
                        if (obj == ifcObjectDefinition)
                        {
                            continue;
                        }
                        else
                        {
                            listObjectIsDefinedBy.Add(
                            $"Наименование связи: {relDef.Name}\n" +
                            $"Описание связи: {relDef.Description}\n" +
                            $"Наименование элемента: {obj.Name}\n" +
                            $"Класс IFC: {obj.GetType().Name}\n" +
                            $"GUID: {obj.GlobalId}");
                        }
                    }
                }

                if (listObjectIsDefinedBy.Count > 0)
                {
                    PropertyElement.Add("Связанные объекты (IsDefinedBy)", listObjectIsDefinedBy);
                }
            }

            //Содержит объекты
            HashSet<object> listObjectContainElement = new HashSet<object>();

            if (ifcObjectDefinition is IIfcSpatialStructureElement IFCStrElem)
            {
                foreach (IIfcRelContainedInSpatialStructure spartialStucture in IFCStrElem.ContainsElements)
                {
                    foreach (IIfcProduct obj in spartialStucture.RelatedElements)
                    {
                        if (obj == ifcObjectDefinition)
                        {
                            continue;
                        }
                        else
                        {
                            listObjectContainElement.Add(
                            $"Наименование связи: {spartialStucture.Name}\n" +
                            $"Описание связи: {spartialStucture.Description}\n" +
                            $"Наименование элемента: {obj.Name}\n" +
                            $"Класс IFC: {obj.GetType().Name}\n" +
                            $"GUID: {obj.GlobalId} ");
                        }
                    }
                }
            }

            if (listObjectContainElement.Count > 0)
            {
                PropertyElement.Add("Содержит объекты (ContainsElements)", listObjectContainElement);
            }

            return PropertyElement;
        }

        #endregion Заполнение свойств элемента

        #region Фабрика

        public static BaseEditorItem CreateEditor(ModelItemIFCObject modelItemIFCObject, ModelIFC modelIFC, IIfcObjectDefinition ifcObjectDefinition)
        {
            if (ifcObjectDefinition is Xbim.Ifc2x3.Kernel.IfcObjectDefinition ifcObjectDefinition2x3)
            {
                return new EditorItemIFC2x3(modelItemIFCObject, modelIFC, ifcObjectDefinition2x3);
            }
            else if (ifcObjectDefinition is Xbim.Ifc4.Kernel.IfcObjectDefinition ifcObjectDefinition4)
            {
                return new EditorItemIFC4(modelItemIFCObject, modelIFC, ifcObjectDefinition4);
            }
            else
            {
                return null;
            }
        }

        #endregion Фабрика

        #endregion Методы
    }
}