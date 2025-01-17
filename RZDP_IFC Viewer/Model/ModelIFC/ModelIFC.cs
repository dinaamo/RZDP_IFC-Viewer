using System.Collections.ObjectModel;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using IFC_Table_View.IFC.ModelItem;
using IFC_Table_View.ViewModels;
using IFC_Viewer.IFC.Base;
using Xbim.Common;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.PropertyResource;

namespace IFC_Table_View.IFC.Model
{
    public class ModelIFC : IDisposable
    {
        private ModelIFC()
        {
        }

        public IfcStore IfcStore { get; private set; }

        public ObservableCollection<BaseModelItemIFC> ModelItems { get; private set; }

        public string FilePath { get; private set; }

        public string FileName { get; private set; }

        public bool SaveFile(string filePath, ReportProgressDelegate ReportProcess)
        {
            if (filePath == null)
            {
                return false;
            }

            IfcStore.SaveAs(filePath, Xbim.IO.StorageType.Ifc, ReportProcess);
            ReportProcess.Invoke(0, "");
            if (IfcStore is not null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool SaveAsXMLFile(string filePath, ReportProgressDelegate ReportProcess)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            IfcStore.SaveAs(filePath, Xbim.IO.StorageType.IfcXml, ReportProcess);
            ReportProcess.Invoke(0, "");
            if (IfcStore is not null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Делегат для фокуса на объекте
        /// </summary>
        public Action<ModelItemIFCObject> ZoomObject;

        /// <summary>
        /// Делегат для выделения объекта
        /// </summary>
        public Action<ModelItemIFCObject> SelectObject;

        /// <summary>
        /// Процесс загрузки
        /// </summary>
        private Action<int, object> ReportProcess;

        /// <summary>
        /// ///////////////
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        //Загружаем базу данных
        public static ModelIFC Create(IfcStore ifcStore, Action<ModelItemIFCObject> ZoomObject, Action<ModelItemIFCObject> SelectObject, Action<int, object> ReportProcess)
        {
            return new ModelIFC().LoadDataBase(ifcStore, ZoomObject, SelectObject, ReportProcess);
        }

        private BaseEditorModel baseEditorModel;

        /// <summary>
        /// Открываем файл
        /// </summary>
        /// <param name="filePath"></param>
        private ModelIFC LoadDataBase(IfcStore ifcStore, Action<ModelItemIFCObject> ZoomObject, Action<ModelItemIFCObject> SelectObject, Action<int, object> ReportProcess)
        {
            this.ZoomObject = ZoomObject;

            this.SelectObject = SelectObject;

            this.ReportProcess = ReportProcess;

            IfcStore = ifcStore;

            FilePath = ifcStore.FileName;

            string[] arr = Regex.Split(FilePath, @"\\");

            FileName = arr[arr.Length - 1];

            using (ITransaction trans = ifcStore.Model.BeginTransaction("ReadFile"))
            {
                try
                {
                    FillCollectionModelItem();
                }
                catch (Exception ex)
                {
                    Dispose();
                    throw new Exception(ex.Message);
                }
                trans.Commit();
            }
            baseEditorModel = BaseEditorModel.CreateEditor(this);

            return this;
        }

        private ModelItemIFCFile FileItem;

        private ObservableCollection<BaseModelReferenceIFC> tempReferenceObjectSet;

        /// <summary>
        /// Заполняем коллекцию элементов дерева модели
        /// </summary>
        private void FillCollectionModelItem()
        {
            ModelItems = new ObservableCollection<BaseModelItemIFC>();

            IIfcProject Project = IfcStore.Instances.FirstOrDefault<IIfcProject>();

            if (Project is null)
            {
                throw new Exception("Не найден корневой элемент IFCProject");
            }
            //Добавляем в дерево первым элементом файл
            FileItem = new ModelItemIFCFile(IfcStore, Project, this);
            ModelItems.Add(FileItem);

            //Ищем все ссылочные элементы в файле и заполняем временную коллекцию
            List<IfcObjectReferenceSelect> referenceObjectSet = IfcStore.Instances.OfType<IfcObjectReferenceSelect>().
                                            Where(it => it is IIfcDocumentReference || it is IIfcTable).ToList();

            int countElements = IfcStore.Instances.OfType<IIfcSpatialStructureElement>().Count();
            countElements += IfcStore.Instances.OfType<IIfcObjectDefinition>().Count();

            countToPresent = 100d / countElements;

            tempReferenceObjectSet = new ObservableCollection<BaseModelReferenceIFC>();
            if (referenceObjectSet.Count > 0)
            {
                countToPresent = 100 / referenceObjectSet.Count();

                for (int i = 0; i < referenceObjectSet.Count(); i++)
                {
                    tempReferenceObjectSet.Add(CreateNewModelReference(referenceObjectSet[i]));
                    UpdateProgress((int)countToPresent * i);
                }
            }

            //Составляем дерево объектов модели
            CreationHierarchyIFCObjects(FileItem.Project, FileItem.ModelItems, null);

            //После того как составили дерево объектов к нему добавляем таблицы
            foreach (BaseModelReferenceIFC tableItem in tempReferenceObjectSet)
            {
                FileItem.ModelItems.Add(tableItem);
            }
            UpdateProgress(100);
        }

        private double countToPresent;
        private int counter = 0;

        private void UpdateProgress(int present)
        {
            ReportProcess.Invoke(present, "Составление дерева элементов");
        }

        /// <summary>
        /// Составляем дерево элементов модели
        /// </summary>
        /// <param name="objDef"></param>
        /// <param name="collection"></param>
        /// <param name="topElement"></param>
        private void CreationHierarchyIFCObjects(IIfcObjectDefinition objDef, ObservableCollection<BaseModelItemIFC> collection, ModelItemIFCObject topElement)
        {
            ModelItemIFCObject nestItem = new ModelItemIFCObject(objDef, topElement, this);

            //Проверяем, что в элементе есть ссылки на таблицы. Если есть то добавляем к таблицам ссылку на элемент
            nestItem.AddReferenceToTheObjectReferenceToLoad(tempReferenceObjectSet);

            collection.Add(nestItem);

            UpdateProgress((int)(countToPresent * ++counter));

            IIfcSpatialStructureElement spatialElement = objDef as IIfcSpatialStructureElement;

            if (spatialElement != null)
            {
                foreach (IIfcObjectDefinition obj in spatialElement.ContainsElements.SelectMany(it => it.RelatedElements).OfType<IIfcObjectDefinition>())
                {
                    CreationHierarchyIFCObjects(obj, nestItem.ModelItems, nestItem);
                }
            }

            foreach (IIfcObjectDefinition obj in objDef.IsDecomposedBy.SelectMany(r => r.RelatedObjects))
            {
                CreationHierarchyIFCObjects(obj, nestItem.ModelItems, nestItem);
            }
        }

        /// <summary>
        /// Добавляем к дереву элементов ссылочные объекты
        /// </summary>
        /// <param name="referenceObjectSet"></param>
        /// <returns></returns>
        private BaseModelReferenceIFC CreateNewModelReference(IfcObjectReferenceSelect referenceObject)
        {
            if (referenceObject is IIfcTable ifcTable)
            {
                return new ModelItemIFCTable(IfcStore, ifcTable, this);
            }
            else if (referenceObject is IIfcDocumentReference ifcDocumentReference)
            {
                return new ModelItemDocumentReference(IfcStore, ifcDocumentReference, this);
            }
            else
            {
                return null;
            }
        }



        /// <summary>
        /// Удаляем таблицу иил документ
        /// </summary>
        /// <param name="tableToDelete"></param>
        public void DeleteIFCObjectReferenceSelect(BaseModelReferenceIFC modelReference)
        {
            using (ITransaction trans = IfcStore.Model.BeginTransaction("DeleteIFCObjectReferenceSelect"))
            {
                #region Удаляем ссылки от ObjectReferenceSelect на элементы дерева и обратно
                IEnumerable<ModelItemIFCObject>? referenceToDelete = modelReference.PropertyElement?.
                                        Where(it => it.Key == "Ссылки на объекты")?.
                                        SelectMany(it => it.Value).
                                        Cast<ModelItemIFCObject>();

                if (referenceToDelete != null)
                {
                    foreach (ModelItemIFCObject modelObject in referenceToDelete.ToArray())
                    {
                        modelObject.DeleteReferenceToTheObject(new List<BaseModelReferenceIFC>() { modelReference });
                        modelReference.DeleteReferenceToTheElement(modelObject);
                    }
                }
                #endregion

                //#region Удаляем из наборов характеристики которые ссылаются на ObjectReferenceSelect
                ////IEnumerable<IIfcPropertySet> PropertySetCollection = IfcStore.Instances.OfType<IIfcObject>().
                ////SelectMany(it => it.IsDefinedBy.
                ////Select(obj => obj.RelatingPropertyDefinition).
                ////OfType<IIfcPropertySet>()).
                ////Where(it => it.HasProperties.Select(pr => pr.GetType() == typeof(IIfcPropertyReferenceValue)) != null).
                ////Where(it => it.HasProperties.Select(pr => ((IIfcPropertyReferenceValue)pr).PropertyReference == modelReference.GetReference()) != null);

                ////foreach (IIfcPropertySet PropertySet in PropertySetCollection.ToArray())
                ////{
                ////    IIfcProperty deletedPoperty = PropertySet.HasProperties.
                ////        Where(it => it.GetType() == typeof(IIfcPropertyReferenceValue)).
                ////        FirstOrDefault(pr => ((IIfcPropertyReferenceValue)pr).PropertyReference == modelReference.GetReference());
                ////    PropertySet.HasProperties.Remove(deletedPoperty);

                ////    //Если в наборе больше нет свойств то удаляем его
                ////    if (PropertySet.HasProperties.Count() == 0)
                ////    {
                ////        IfcStore.Delete(PropertySet);
                ////    }
                ////}
                //#endregion

                #region Удаляем объект из дерева
                BaseModelReferenceIFC modelItemToDelete = ModelItems[0].ModelItems.OfType<BaseModelReferenceIFC>().FirstOrDefault(it =>
                {
                    if (it.GetReference() != null)
                    {
                        return it.GetReference().Equals(modelReference.GetReference());
                    }
                    return false;
                });

                ModelItems[0].ModelItems.Remove(modelItemToDelete);
                #endregion

                # region Удаляем элемент ObjectReferenceSelect
                IfcStore.Delete(modelReference.GetReference());
                #endregion

                trans.Commit();
            }
        }

        ///// <summary>
        ///// Удаляем ссылку на документ
        ///// </summary>
        ///// <param name="documentReference"></param>
        //public void DeleteReferenceToDocument(IIfcDocumentReference documentReference)
        //{
        //    using (ITransaction trans = IfcStore.Model.BeginTransaction("DeleteReferenceToDocument"))
        //    {





        //        baseEditorModel.DeleteReferenceToDocument(documentReference);

        //        BaseModelReferenceIFC modelItemToDelete = ModelItems[0].ModelItems.OfType<BaseModelReferenceIFC>().FirstOrDefault(it =>
        //        {
        //            if (it.GetReference() != null)
        //            {
        //                return it.GetReference().Equals(documentReference);
        //            }
        //            return false;
        //        });

        //        ModelItems[0].ModelItems.Remove(modelItemToDelete);

        //        trans.Commit();
        //    }
        //}

        public void DeleteIFCEntity(IPersistEntity persistEntity)
        {
            if (!IfcStore.Model.IsTransactional)
            {
                IfcStore.Delete(persistEntity);
            }
            else
            {
                using (ITransaction trans = IfcStore.Model.BeginTransaction("DeleteIFCEntity"))
                {
                    IfcStore.Delete(persistEntity);

                    trans.Commit();
                }
            }
        }

        public void DeleteReferenceCommand(BaseModelReferenceIFC ModelReference)
        {
           
        }

        /// <summary>
        /// Добавляем в модель таблицу
        /// </summary>
        /// <param name="dataTable"></param>
        public void CreateNewIFCTable(DataTable dataTable)
        {
            using (ITransaction trans = IfcStore.Model.BeginTransaction("CreateNewIFCTable"))
            {
                IIfcTable ifcTable = baseEditorModel.CreateNewIFCTable(dataTable);

                BaseModelReferenceIFC tempTableItem = CreateNewModelReference(ifcTable);

                FileItem.ModelItems.Add(tempTableItem);
                trans.Commit();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="location">Путь к документу</param>
        /// <param name="referenseName">Имя ссылки</param>
        /// <param name="positionInDocumente">Позиция в документе</param>
        public void CreateNewIFCDocumentInformation(ModelDocument modelDocument)
        {
            using (ITransaction trans = IfcStore.Model.BeginTransaction("CreateNewIFCDocumentInformation"))
            {
                IIfcDocumentReference ifcDocumentReference = baseEditorModel.CreateNewIFCDocumentInformation(modelDocument);

                BaseModelReferenceIFC tempDocumentItem = CreateNewModelReference(ifcDocumentReference);

                FileItem.ModelItems.Add(tempDocumentItem);

                trans.Commit();
            }
        }

        /// <summary>
        /// Добавить ссылки на элементы
        /// </summary>
        /// <param name="modelObjects"></param>
        /// <param name="referenceObjects"></param>
        public void AddReferenceToTheObject(List<ModelItemIFCObject> modelObjects, List<BaseModelReferenceIFC> referenceObjects)
        {
            using (ITransaction trans = IfcStore.Model.BeginTransaction("AddReferenceToTheObject"))
            {
                foreach (ModelItemIFCObject modelItem in modelObjects)
                {
                    modelItem.AddReferenceToTheObject(referenceObjects);
                }

                trans.Commit();
            }
        }

        /// <summary>
        /// Удалить ссылки на элементы
        /// </summary>
        /// <param name="modelObjects"></param>
        /// <param name="referenceObjects"></param>
        public void DeleteReferenceToTheObject(List<ModelItemIFCObject> modelObjects, List<BaseModelReferenceIFC> referenceObjects)
        {
            using (ITransaction trans = IfcStore.Model.BeginTransaction("DeleteReferenceToTheObject"))
            {
                foreach (ModelItemIFCObject modelItem in modelObjects)
                {
                    modelItem.DeleteReferenceToTheObject(referenceObjects);
                }

                trans.Commit();
            }
        }

        public void ChangeName<T>(List<(T, string)> tupleCollection)
        {
            using (ITransaction trans = IfcStore.Model.BeginTransaction("ChangeNameObjects"))
            {
                foreach ((T, string) tuple in tupleCollection)
                {
                    if (tuple.Item1 is IIfcRoot root)
                    {
                        root.Name = tuple.Item2;
                    }
                    else if (tuple.Item1 is IIfcProperty property)
                    {
                        property.Name = tuple.Item2;
                    }
                    else if (tuple.Item1 is IIfcTable table)
                    {
                        table.Name = tuple.Item2;
                    }
                    //else if (tuple.Item1 is IIfcDocumentReference documentReference)
                    //{
                    //    documentReference.Name = tuple.Item2;
                    //}
                    else if (tuple.Item1 is IIfcPhysicalQuantity physicalQuantity)
                    {
                        physicalQuantity.Name = tuple.Item2;
                    }
                }

                trans.Commit();
            }
        }

        public void ChangeValue(List<(Action<string>, string)> tupleCollection)
        {
            using (ITransaction trans = IfcStore.Model.BeginTransaction("ChangeValueProperty"))
            {
                foreach ((Action<string>, string) tuple in tupleCollection)
                {
                    tuple.Item1(tuple.Item2);
                }

                trans.Commit();
            }
        }

        public void Dispose()
        {
            IfcStore.Dispose();
        }
    }
}