using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;
using System.Windows;
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
        private ModelIFC(IfcStore ifcStore)
        {
            IfcStore = ifcStore;
            IfcStore.EntityDeleted += IfcStore_EntityDeleted;
            IfcStore.EntityModified += IfcStore_EntityModified;
            IfcStore.EntityNew += IfcStore_EntityNew;
        }

        private void IfcStore_EntityNew(IPersistEntity entity)
        {
            IsEditModel = true;
        }

        private void IfcStore_EntityModified(IPersistEntity entity, int property)
        {
            IsEditModel = true;
        }

        private void IfcStore_EntityDeleted(IPersistEntity entity)
        {
            IsEditModel = true;
        }

        public bool IsEditModel { get; private set; } = false;
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
            ReportProcess?.Invoke(0, "");
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
        public Action<IPersistEntity> ZoomObject;

        /// <summary>
        /// Делегат для выделения объекта
        /// </summary>
        public Action<IPersistEntity> SelectObject;

        /// <summary>
        /// Процесс загрузки
        /// </summary>
        private BackgroundWorker backgroundWorker;


        /// <summary>
        /// Процесс загрузки
        /// </summary>
        private Action<IEnumerable<IPersistEntity>> RefreshDCAfterDeleteEntity;

        /// <summary>
        /// ///////////////
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        //Загружаем базу данных
        public static ModelIFC Create(IfcStore ifcStore, Action<IPersistEntity> ZoomObject, Action<IPersistEntity> SelectObject, BackgroundWorker ReportProcess, Action<IEnumerable<IPersistEntity>> RefreshDCAfterDeleteEntity)
        {
            return new ModelIFC(ifcStore).LoadDataBase(ifcStore, ZoomObject, SelectObject, ReportProcess, RefreshDCAfterDeleteEntity);
        }

        private BaseEditorModel baseEditorModel;

        /// <summary>
        /// Открываем файл
        /// </summary>
        /// <param name="filePath"></param>
        private ModelIFC LoadDataBase(IfcStore ifcStore, Action<IPersistEntity> ZoomObject, Action<IPersistEntity> SelectObject, BackgroundWorker ReportProcess, Action<IEnumerable<IPersistEntity>> RefreshDCAfterDeleteEntity)
        {
            this.ZoomObject = ZoomObject;

            this.SelectObject = SelectObject;

            this.backgroundWorker = ReportProcess;

            this.RefreshDCAfterDeleteEntity = RefreshDCAfterDeleteEntity;

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
            IsEditModel = false;
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
                    UpdateProgress((int)countToPresent * i, "Составление дерева элементов");
                }
            }

            //Составляем дерево объектов модели
            CreationHierarchyIFCObjects(FileItem.Project, FileItem.ModelItems, FileItem);

            //После того как составили дерево объектов к нему добавляем таблицы
            foreach (BaseModelReferenceIFC tableItem in tempReferenceObjectSet)
            {
                FileItem.ModelItems.Add(tableItem);
            }
            UpdateProgress(100, "Составление дерева элементов");
        }

        private double countToPresent;
        private int counter = 0;

        private void UpdateProgress(int present, string message = null)
        {
            backgroundWorker.ReportProgress(present, message);
        }

        /// <summary>
        /// Составляем дерево элементов модели
        /// </summary>
        /// <param name="objDef"></param>
        /// <param name="collection"></param>
        /// <param name="topElement"></param>
        private void CreationHierarchyIFCObjects(IIfcObjectDefinition objDef, ObservableCollection<BaseModelItemIFC> collection, BaseModelItemIFC topElement)
        {
            ModelItemIFCObject nestItem = new ModelItemIFCObject(objDef, topElement, this);

            //Проверяем, что в элементе есть ссылки на таблицы. Если есть то добавляем к таблицам ссылку на элемент
            nestItem.AddReferenceToTheObjectReferenceToLoad(tempReferenceObjectSet);

            collection.Add(nestItem);

            UpdateProgress((int)(countToPresent * ++counter), "Составление дерева элементов");

            if (objDef is IIfcSpatialStructureElement spatialElement)
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



        public void DeleteIFCEntity(IPersistEntity persistEntity)
        {
            try
            {
                if (IfcStore.Model.CurrentTransaction is null)
                {
                    using (ITransaction trans = IfcStore.Model.BeginTransaction("DeleteIFCEntity"))
                    {
                        IfcStore.Delete(persistEntity);

                        trans.Commit();
                    }
                }
                else
                {
                    IfcStore.Delete(persistEntity);

                 }
            }
            catch (StackOverflowException sEx)
            {

                MessageBox.Show(sEx.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void DeleteModelObjects(List<ModelItemIFCObject> modelItemIFCObjectSet)
        {
            var tt = DeleteModelObjectsBackground;
            backgroundWorker.DoWork += DeleteModelObjectsBackground;
            backgroundWorker.RunWorkerAsync(modelItemIFCObjectSet);
        }


        void DeleteModelObjectsBackground(object sender, DoWorkEventArgs args)
        {
            if (args.Argument is List<ModelItemIFCObject> modelItemIFCObjectSet)
            {
                using (ITransaction trans = IfcStore.Model.BeginTransaction("DeleteModelObjects"))
                {
                    var entityToDelete = modelItemIFCObjectSet.Select(it => it.GetIFCObject());
                    countToPresent = 100d / modelItemIFCObjectSet.Count;
                    counter = 0;
                    foreach (var modelItemIFCObject in modelItemIFCObjectSet)
                    {
                        UpdateProgress((int)(countToPresent * ++counter), "Удаление элементов");
                        DeleteIFCEntity(modelItemIFCObject.GetIFCObject());
                        Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            modelItemIFCObject.TopElement.ModelItems.Remove(modelItemIFCObject);
                        });
                    }
                    RefreshDCAfterDeleteEntity(entityToDelete);
                    UpdateProgress(0);
                    trans.Commit();
                }
            }
            backgroundWorker.DoWork -= DeleteModelObjectsBackground;
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

        public void AddEntity(List<Action> actionSet)
        {
            using (ITransaction trans = IfcStore.Model.BeginTransaction("AddEntity"))
            {
                foreach (Action action in actionSet)
                {
                    action();
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