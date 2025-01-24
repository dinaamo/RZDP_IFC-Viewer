using System.Collections.ObjectModel;
using Editor_IFC;
using IFC_Table_View.IFC.Model;
using IFC_Table_View.IFC.ModelItem;
using IFC_Viewer.IFC.Base;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.PropertyResource;

namespace IFC_Viewer.IFC.ModelItem
{
    public class EditorItemIFC4 : BaseEditorItem
    {
        private IfcObjectDefinition _ifcObjectDefinition;

        public EditorItemIFC4(ModelItemIFCObject modelItemIFCObject, ModelIFC modelIFC, IfcObjectDefinition IFCObjectDefinition) : base(modelItemIFCObject, modelIFC, IFCObjectDefinition)
        {
            this._ifcObjectDefinition = IFCObjectDefinition;
        }
        #region Создать набор характеристик
        public override void CreateNewPropertySet()
        {
            IfcPropertySet newPropertySet = ModelIFC.IfcStore.Model.Instances.New<IfcPropertySet>(prS =>
            { prS.Name = "Новый набор"; });
            AddPropertySet(newPropertySet);
        }
        #endregion

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

        #region Удаление ссылок

        //public override List<BasePropertySetDefinition> DeleteReferenceToTheObject(List<BaseModelReferenceIFC> CollectionModelitemReferenceToDelete)
        //{
        //    //Получаем необходимый набор характеристик
        //    List<BasePropertySetDefinition> PropSetReferenceCollection = CollectionPropertySet.
        //                                        Where(it => it.IFCPropertySetDefinition.GetType() == typeof(IIfcPropertySet)).
        //                                        Where(it => ((IIfcPropertySet)it.IFCPropertySetDefinition).HasProperties.Any(pr => pr.GetType() == typeof(IIfcPropertyReferenceValue))).ToList();


        //    if (PropSetReferenceCollection.Count == 0)
        //    {
        //        throw new ArgumentNullException("Не найден набор с ссылками");
        //    }

        //    //Удаляем ссылки

        //    foreach (BaseModelReferenceIFC modelReference in CollectionModelitemReferenceToDelete)
        //    {
        //        foreach (BasePropertySetDefinition propertySet in PropSetReferenceCollection.ToArray())
        //        {

        //            IIfcPropertyReferenceValue propertyToDelete = ((IIfcPropertySet)propertySet.IFCPropertySetDefinition).HasProperties
        //                                                                                .OfType<IIfcPropertyReferenceValue>()
        //                                                                                .FirstOrDefault(it => it.PropertyReference == modelReference.GetReference());

        //            if (propertyToDelete is null)
        //            { continue; }

        //            modelReference.DeleteReferenceToTheElement(modelItemIFCObject);
        //            foreach (var PropSetReference in PropSetReferenceCollection)
        //            {
        //                ((IIfcPropertySet)PropSetReference.IFCPropertySetDefinition).HasProperties.Remove(propertyToDelete);
        //                PropSetReferenceCollection.Remove(propertySet);
        //            }
        //        }
        //    }


        //    return PropSetReferenceCollection;
        //}

        #endregion


        public override void DeletePropertySet(BasePropertySetDefinition PropertySetDefinition)
        {
            if (_ifcObjectDefinition is IfcObject ifcObject)
            {
                IfcRelDefinesByProperties? rpRelDef = ifcObject.IsDefinedBy.FirstOrDefault(prDef => prDef.RelatingPropertyDefinition == PropertySetDefinition.IFCPropertySetDefinition);

                if (rpRelDef != null)
                {
                    ModelIFC.DeleteIFCEntity(rpRelDef);
                    
                }
                ModelIFC.DeleteIFCEntity(PropertySetDefinition.IFCPropertySetDefinition);
            }
            else if (ifcObjectDefinition is IfcContext ifcContext)
            {
                IfcRelDefinesByProperties? rpRelDef =  ifcContext.IsDefinedBy.FirstOrDefault(prDef => prDef.RelatingPropertyDefinition == PropertySetDefinition.IFCPropertySetDefinition);

                if (rpRelDef != null)
                {
                    ModelIFC.DeleteIFCEntity(rpRelDef);

                }
                ModelIFC.DeleteIFCEntity(PropertySetDefinition.IFCPropertySetDefinition);
            }
        }

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
        //    else if (ifcObjectDefinition is IfcContext context)
        //    {
        //        IEnumerable<IfcPropertySetDefinition> collectionProperty = context.IsDefinedBy.Select(it => it.RelatingPropertyDefinition).OfType<IfcPropertySetDefinition>();

        //        foreach (IfcPropertySetDefinition propSetIsObj in collectionProperty)
        //        {
        //            CollectionPropertySet.Add(propSetIsObj);
        //        }
        //    }

        //    return true;
        //}

        #endregion Заполнение характеристик элемента

        #region Добавить набор характеристик

        protected override void AddPropertySet(IIfcPropertySetDefinition iIfcPropertySet)
        {
            //IfcPropertySet? ifcPropertySet = iIfcPropertySet as IfcPropertySet;

            //if (ifcObjectDefinition is IfcObject ifcObject)
            //{
            IfcRelDefinesByProperties ifcRelDefinesByProperties = ModelIFC.IfcStore.Instances.New<IfcRelDefinesByProperties>(relDef =>
            {
                relDef.RelatedObjects.Add(_ifcObjectDefinition);
                relDef.RelatingPropertyDefinition = iIfcPropertySet;
            });
            //}
            //else if (ifcObjectDefinition is IfcContext ifcContext)
            //{
            //    IfcRelDefinesByProperties ifcRelDefinesByProperties = ModelIFC.IfcStore.Instances.New<IfcRelDefinesByProperties>(relDef =>
            //    {
            //        relDef.RelatedObjects.Add(IFCObjectDefinition);
            //        relDef.RelatingPropertyDefinition = ifcPropertySet;
            //    });
            //}
    }

        #endregion Добавить набор характеристик
    }
}

//#endregion