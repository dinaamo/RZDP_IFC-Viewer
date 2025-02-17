using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Input;
using Editor_IFC;
using RZDP_IFC_Viewer.IFC.Model;
using RZDP_IFC_Viewer.Infracrucrure;
using RZDP_IFC_Viewer.Infracrucrure.Commands;
using RZDP_IFC_Viewer.Infracrucrure.FindObjectException;
using RZDP_IFC_Viewer.View.Windows;
using IFC_Viewer.IFC.Base;
using IFC_Viewer.View.Windows;
using RZDP_IFC_Viewer.View.Windows;
using Xbim.Ifc4.Interfaces;
using static Microsoft.Isam.Esent.Interop.EnumeratedColumn;
using Xbim.Common;

namespace RZDP_IFC_Viewer.IFC.ModelItem
{
    public class ModelItemIFCObject : BaseModelItemIFC
    {
        public BaseModelItemIFC TopElement {  get; set; }
        /// <summary>
        /// Конструктор
        /// </summary>
        public ModelItemIFCObject(IIfcObjectDefinition IFCObject, BaseModelItemIFC TopElement, ModelIFC modelIFC) : base(modelIFC, IFCObject, TopElement)
        {
            //Если есть элемент выше по дереву то подключаем к нему обработчик события изменения состояния элемента
            this.TopElement = TopElement;
            if (TopElement is ModelItemIFCObject modelItemIFCObject)
            {
                PropertyReferenceChanged += modelItemIFCObject.ChangePropertyReference;
            }

            IFCObjectDefinition = IFCObject;

            ZoomElementsCommand = new ActionCommand(
                   OnZoomElementsCommandExecuted,
                   CanZoomElementsCommandExecute);

            AddPropertyCommand = new ActionCommand(
                   OnAddPropertyCommandExecuted,
                   CanAddPropertyCommandExecute);

            AddPropertySetCommand = new ActionCommand(
                   OnAddPropertySetCommandExecuted,
                   CanAddPropertySetCommandExecute);

            PaintElementsCommand = new ActionCommand(
                   OnPaintElementsCommandExecuted,
                   CanPaintElementsCommandExecute);

            SearchAndEditElementsCommand = new ActionCommand(
                   OnSearchAndEditElementsCommandExecuted,
                   CanSearchAndEditElementsCommandExecute);

            EditElementsCommand = new ActionCommand(
                   OnEditElementsCommandExecuted,
                   CanEditElementsCommandExecute);

            ResetSearchCommand = new ActionCommand(
                OnResetSearchCommandExecuted,
                CanResetSearchCommandExecute);

            AddReferenceToTheTable = new ActionCommand(
                    OnAddReferenceToTheTable,
                    CanAddReferenceToTheTable);

            DeleteReferenceToTheTable = new ActionCommand(
                OnDeleteReferenceToTheTable,
                CanDeleteReferenceToTheTable);

            DeleteModelObject = new ActionCommand(
                OnDeleteModelObject,
                CanDeleteModelObject);

            HideSelectedModelObjectCommand = new ActionCommand(
                OnHideSelectedModelObjectCommand,
                CanHideSelectedModelObjectCommand);

            IsolateSelectedModelObjectCommand = new ActionCommand(
                OnIsolateSelectedModelObjectCommand,
                CanIsolateSelectedModelObjectCommand);

            SelectedModelObjectCommand = new ActionCommand(
                OnSelectedModelObjectCommand,
                CanSelectedModelObjectCommand);


            InitializationModelObject();
        }

        #region Комманды

        #region Фокус на элемент

        public ICommand ZoomElementsCommand { get; }

        private void OnZoomElementsCommandExecuted(object o)
        {
            //Вариант для выбора под элементов
            Model.ZoomObjects(SelectionNestedItems(this).Select(it => it.IFCObjectDefinition));

            //Model.ZoomObjects(new List<IIfcRoot>() { IFCObjectDefinition });
        }

        private bool CanZoomElementsCommandExecute(object o)
        {
            return true;
        }

        #endregion Фокус на элемент

        #region Добавить свойство

        public ICommand AddPropertyCommand { get; }

        private void OnAddPropertyCommandExecuted(object o)
        {
            if (o is BasePropertySetDefinition propertySetDefinition)
            {
                Model.ActionInTransaction(new List<Action> { propertySetDefinition.CreateNewProperty});
            }
        }

        private bool CanAddPropertyCommandExecute(object o)
        {
            return o is not null;
        }

        #endregion Добавить свойство

        #region Добавить набор характеристик

        public ICommand AddPropertySetCommand { get; }

        private void OnAddPropertySetCommandExecuted(object o)
        {
            Model.ActionInTransaction(new List<Action> { AddPropertySet });
            OnPropertyChanged("CollectionPropertySet");
        }


        private bool CanAddPropertySetCommandExecute(object o)
        {
            return this is not null;
        }

        void AddPropertySet()
        {
            ModelObjectEditor.CreateNewPropertySet();
        }

        #endregion Добавить набор характеристик

        #region Выделить элемент

        public ICommand PaintElementsCommand { get; }

        private void OnPaintElementsCommandExecuted(object o)
        {
            IsPaint = true;
        }

        private bool CanPaintElementsCommandExecute(object o)
        {
            return true;
        }

        #endregion Выделить элемент

        #region Групповые операции

        public ICommand SearchAndEditElementsCommand { get; }

        private void OnSearchAndEditElementsCommandExecuted(object o)
        {
            if (o is ModelItemIFCObject modelItem)
            {
                SearchAndEditWindow.CreateWindowSearch(SelectionNestedItems(modelItem));
            }
        }

        private bool CanSearchAndEditElementsCommandExecute(object o)
        {
            return true;
        }

        #endregion Групповые операции

        #region Редактирование

        public ICommand EditElementsCommand { get; }

        private void OnEditElementsCommandExecuted(object o)
        {
            if (o is ModelItemIFCObject modelItem)
            {
                EditorWindow.CreateWindowSearch(modelItem);
            }
        }

        private bool CanEditElementsCommandExecute(object o)
        {
            return true;
        }

        #endregion Редактирование

        #region Сброс выделения

        public ICommand ResetSearchCommand { get; }

        private void OnResetSearchCommandExecuted(object o)
        {
            if (o is ModelItemIFCObject modelItem)
            {
                SelectionNestedItems(modelItem).ForEach(it => it.IsPaint = false);
            }
        }

        private bool CanResetSearchCommandExecute(object o)
        {
            return true;
        }

        #endregion Сброс выделения

        #region Добавить к элементу связь со ссылочным объектом

        public ICommand AddReferenceToTheTable { get; }

        private void OnAddReferenceToTheTable(object o)
        {
            if (o is ModelItemIFCObject modelObject)
            {
                List<BaseModelReferenceIFC> collectionModelReference = Model.ModelItems[0].ModelItems.
                                                    OfType<BaseModelReferenceIFC>().
                                                    ToList();
                
                SelectReferenceObjectWindow.CreateSelectReferenceObjectWindow(new List<ModelItemIFCObject> { this }, collectionModelReference, Model.AddReferenceToTheObject);

            }
        }

        private bool CanAddReferenceToTheTable(object o)
        {
            return true;
        }

        #endregion Добавить к элементу связь со ссылочным объектом

        #region Удалить ссылки на документ или таблицу

        public ICommand DeleteReferenceToTheTable { get; }

        private void OnDeleteReferenceToTheTable(object o)
        {
            List<BaseModelReferenceIFC> collectionModelReference = Model.ModelItems[0].ModelItems.
                                                OfType<BaseModelReferenceIFC>().
                                                ToList();

            SelectReferenceObjectWindow.CreateSelectReferenceObjectWindow(new List<ModelItemIFCObject> { this }, collectionModelReference, Model.DeleteReferenceToTheObject);

        }

        private bool CanDeleteReferenceToTheTable(object o)
        {
            return true;
        }

        #endregion Удалить ссылки на документ или таблицу

        #region Удалить элемент

        public ICommand DeleteModelObject { get; }

        private void OnDeleteModelObject(object o)
        {
            
            if(IFCObjectDefinition is IIfcProject)
            {
                MessageBox.Show($"Нельзя удалить IfcProject\n ", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                return;
            }

            List<ModelItemIFCObject> CollectionNestedItems = SelectionNestedItems(this);

            MessageBoxResult result = MessageBox.Show($"Удалить элемент?\n" +
                $"Будет удалено элементов: {CollectionNestedItems.Count}.", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
            { return; }


            Model.DeleteModelObjects(CollectionNestedItems);
        }

        private bool CanDeleteModelObject(object o)
        {
            return true;
        }

        #endregion Удалить элемент

        #region Скрыть элемент

        public ICommand HideSelectedModelObjectCommand { get; }

        private void OnHideSelectedModelObjectCommand(object o)
        {
            Model.HideSelected(SelectionNestedItems(this).Select(it => it.IFCObjectDefinition));
        }

        private bool CanHideSelectedModelObjectCommand(object o)
        {
            return true;
        }

        #endregion Скрыть элемент

        #region Полказать элемент

        public ICommand SelectedModelObjectCommand { get; }

        private void OnSelectedModelObjectCommand(object o)
        {
            Model.ShowSelected(SelectionNestedItems(this).Select(it=> it.IFCObjectDefinition));
        }

        private bool CanSelectedModelObjectCommand(object o)
        {
            return true;
        }

        #endregion Полказать элемент

        #region Изолировать элемент

        public ICommand IsolateSelectedModelObjectCommand { get; }

        private void OnIsolateSelectedModelObjectCommand(object o)
        {
            Model.IsolateSelected(SelectionNestedItems(this).Select(it => it.IFCObjectDefinition));
        }

        private bool CanIsolateSelectedModelObjectCommand(object o)
        {
            return true;
        }

        #endregion Изолировать элемент

 

        #endregion Комманды

        #region Методы

        #region Выделить элемент

        public void SelectElement()
        {
            //Вариант для выбора под элементов
            //Model.SelectObjects(SelectionNestedItems(this).Select(it => it.IFCObjectDefinition));

            Model.SelectObjects(new List<IIfcRoot>() { IFCObjectDefinition });
        }

        #endregion Выделить элемент


        #region Действия с наборами характеристик
        public void DeletePropertySet(BasePropertySetDefinition propertySetDefinition)
        {
            Model.DeleteIFCEntity(ModelObjectEditor.DeletePropertySet(propertySetDefinition.IFCPropertySetDefinition));

            OnPropertyChanged("CollectionPropertySet");
        }


        public void AddDublicatePropertySet(BasePropertySetDefinition propertySetDefinition)
        {
            Model.ActionInTransactionForPropertySet(new List<(Action<BasePropertySetDefinition>, BasePropertySetDefinition)>
            {
                (ModelObjectEditor.AddDublicatePropertySet, propertySetDefinition)
            });
            OnPropertyChanged("CollectionPropertySet");
        }

        public void UnpinPropertySet(BasePropertySetDefinition propertySetDefinition)
        {
            Model.ActionInTransactionForPropertySet(new List<(Action<BasePropertySetDefinition>, BasePropertySetDefinition)>
            {
                (ModelObjectEditor.UnpinPropertySet, propertySetDefinition)
            });
            OnPropertyChanged("CollectionPropertySet");
        }
        #endregion

        public IIfcObjectDefinition GetIFCObjectDefinition()
        {
            return IFCObjectDefinition;
        }

        /// <summary>
        /// Инициализация элементов объекта модели
        /// </summary>
        private void InitializationModelObject()
        {
            ModelObjectEditor = BaseEditorItem.CreateEditor(this, ModelIFC, IFCObjectDefinition);


            PropertyElement = ModelObjectEditor.GetPropertyObject();

            int? countPropRef = CollectionPropertySet?.SelectMany(it => it.PropertyCollection).
                                                                Select(it => it.Property).
                                                                OfType<IIfcPropertyReferenceValue>().Count();

            if (countPropRef > 0)
            {
                IsContainPropertyReference = true;
                PropertyReferenceChanged?.Invoke(this, new PropertyReferenceChangedEventArg(IsContainPropertyReference));
            }
       
        }

        //private async Task InitializationModelObjectAsync()
        //{
        //    await Task.Run(InitializationModelObject);
        //}

        public event EventHandler<PropertyReferenceChangedEventArg> PropertyReferenceChanged;

        /// <summary>
        /// Прокидываем по дереву вверх наличие ссылок
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        public void ChangePropertyReference(object obj, PropertyReferenceChangedEventArg e)
        {
            if (e.IsContainPropertyDownTreeReference)
            {
                IsContainPropertyReferenceDownTree = true;
            }
            else if (!e.IsContainPropertyDownTreeReference)
            {
                //Проверка наличия ниже по дереву ссылок
                bool searchResult = SelectionNestedItems(this)
                                        .Where(it => it != this)
                                        .Any(it => it.IsContainPropertyReference);

                if (searchResult)
                {
                    IsContainPropertyReferenceDownTree = true;
                }
                else
                {
                    IsContainPropertyReferenceDownTree = false;
                }
            }
            //else
            //{
            //    IsContainPropertyReferenceDownTree = false;
            //}

            PropertyReferenceChanged?.Invoke(this, new PropertyReferenceChangedEventArg(IsContainPropertyReferenceDownTree));
        }

        public BaseEditorItem ModelObjectEditor { get; private set; }

        /// <summary>
        /// Удаление ссылок
        /// </summary>
        /// <param name="CollectionModelReferenceToDelete"></param>
        public void DeleteReferenceToTheObject(IEnumerable<BaseModelReferenceIFC> CollectionModelReferenceToDelete)
        {
            try
            {
                //Удаляем нужные параметры и наборы
                int countPropSetContainsReference = ModelObjectEditor.DeleteReferenceToTheObject(CollectionModelReferenceToDelete);

                if (countPropSetContainsReference == 0)
                {
                    //Если ссылок в элементе больше нет то ставим флаг, что свойств нет
                    IsContainPropertyReference = false;
                }


                //Прокидываем наверх по дереву состояние флага
                PropertyReferenceChanged?.Invoke(this, new PropertyReferenceChangedEventArg(IsContainPropertyReference));

                //Обновляем коллекцию во вьюшке
                OnPropertyChanged("CollectionPropertySet");
            }
            catch (ArgumentNullException)
            {
                throw;
            }
        }

        /// <summary>
        /// При заполнении дерева, если в элементе есть ссылка на таблицу,
        /// то добавляем к таблице обратную ссылку на элемент
        /// </summary>
        public void AddReferenceToTheObjectReferenceToLoad(ObservableCollection<BaseModelReferenceIFC> ReferenceElementSet)
        {
            var propertyReferenceSet = CollectionPropertySet.SelectMany(it => it.PropertyCollection).
                                                                Select(it => it.Property).
                                                                OfType<IIfcPropertyReferenceValue>();

            foreach (IIfcPropertyReferenceValue propertyReference in propertyReferenceSet)
            {
                foreach (BaseModelReferenceIFC tableItem in ReferenceElementSet)
                {
                    if (propertyReference.PropertyReference?.Equals(tableItem.GetReference()) is true)
                    {
                        tableItem.AddReferenceToTheElement(this);
                    }
                }
            }
        }

        /// <summary>
        /// Добавление ссылок
        /// </summary>
        /// <param name="modelReferenceSet"></param>
        public void AddReferenceToTheObject(List<BaseModelReferenceIFC> modelReferenceSet)
        {
            IsContainPropertyReference = ModelObjectEditor.AddReferenceToTheObject(modelReferenceSet);

            //Прокидываем наверх по дереву событие добавления ссылки
            PropertyReferenceChanged?.Invoke(this, new PropertyReferenceChangedEventArg(IsContainPropertyReference));

            //Обновляем коллекцию во вьюшке
            OnPropertyChanged("CollectionPropertySet");
        }



        /// <summary>
        /// Ищем все элементы в дереве по критериям
        /// </summary>
        /// <param name="topElement"></param>
        /// <param name="foundObjects"></param>
        public static List<ModelItemIFCObject> FindPaintObjects(ModelItemIFCObject topElement)
        {
            return SelectionNestedItems(topElement).Where(it => it.IsPaint).ToList();
        }

        /// <summary>
        /// Ищем один элемент в дереве
        /// </summary>
        public static void FindSingleTreeObject(ModelItemIFCObject topElement, object searchObject)
        {
            foreach (ModelItemIFCObject item in topElement.ModelItems)
            {
                if (item.ItemIFC.Equals(searchObject))
                {
                    //item.ExpandOver();
                    throw new FindObjectException(item);
                }
            }

            foreach (ModelItemIFCObject item in topElement.ModelItems)
            {
                //item.IsExpanded = true;
                FindSingleTreeObject(item, searchObject);
            }
        }

        /// <summary>
        /// Выборка элементов ниже по дереву
        /// </summary>
        /// <param name="modelItem"></param>
        /// <returns></returns>
        public static List<ModelItemIFCObject> SelectionNestedItems(ModelItemIFCObject modelItem)
        {
            List<ModelItemIFCObject> list = new List<ModelItemIFCObject>{modelItem};

            foreach (ModelItemIFCObject nestModelItem in modelItem.ModelItems)
            {
                list.AddRange(SelectionNestedItems(nestModelItem));
            }

            return list;
        }

        public override bool Equals(object? other)
        {
            if (other is ModelItemIFCObject otherModelObject)
            {
                return Equals(otherModelObject.IFCObjectDefinition, IFCObjectDefinition);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return IFCObjectDefinition.GetHashCode();
        }

        #endregion Методы

        #region Свойства

        private readonly IIfcObjectDefinition IFCObjectDefinition;

        public string IFCObjectGUID => IFCObjectDefinition.GlobalId.ToString();

        public string IFCObjectType
        {
            get 
            {
                if (IFCObjectDefinition is IIfcObject ifcobject)
                {
                    return ifcobject.ObjectType;
                }
                else if (IFCObjectDefinition is IIfcContext ifccontext)
                {
                    return ifccontext.ObjectType;
                }
                else
                {
                    return "NotDefined";
                }
            } 
        }

        public string IFCDescription => IFCObjectDefinition.Description ?? "";

        public string IFCObjectName
        {
            get
            {
                return IFCObjectDefinition.Name;
            }
            set
            {
                Model.ChangeName(new List<(IIfcRoot, string)> { (IFCObjectDefinition, value) });
                OnPropertyChanged("IFCObjectName");
            }
        }

        private bool _IsContainPropertyReference { get; set; } = false;

        /// <summary>
        /// Наличие в элементе ссылки
        /// </summary>
        public bool IsContainPropertyReference
        {
            get { return _IsContainPropertyReference; }
            set
            {
                if (!value && !IsContainPropertyReferenceDownTree)
                {
                    IsNotContainAnyReferenceProperty = true;
                }
                else
                {
                    IsNotContainAnyReferenceProperty = false;
                }

                _IsContainPropertyReference = value;
                OnPropertyChanged("IsContainPropertyReference");
            }
        }

        private bool _IsContainPropertyReferenceDownTree;

        /// <summary>
        /// Наличие в ниже по дереву элементов в ссылками
        /// </summary>
        public bool IsContainPropertyReferenceDownTree
        {
            get { return _IsContainPropertyReferenceDownTree; }
            set
            {
                if (!value && !IsContainPropertyReference)
                {
                    IsNotContainAnyReferenceProperty = true;
                }
                else
                {
                    IsNotContainAnyReferenceProperty = false;
                }

                _IsContainPropertyReferenceDownTree = value;
                OnPropertyChanged("IsContainPropertyReferenceDownTree");
            }
        }

        private bool _IsNotContainAnyReferenceProperty = true;

        /// <summary>
        /// Не содержит ни в себе, ни ниже по дереву ссылок
        /// </summary>
        public bool IsNotContainAnyReferenceProperty
        {
            get { return _IsNotContainAnyReferenceProperty; }
            set
            {
                _IsNotContainAnyReferenceProperty = value;
                OnPropertyChanged("IsNotContainAnyReferenceProperty");
            }
        }

        /// <summary>
        /// Покрасить элемент
        /// </summary>
        private bool _IsPaint { get; set; }

        public bool IsPaint
        {
            get { return _IsPaint; }
            set
            {
                _IsPaint = value;
                OnPropertyChanged("IsPaint");
            }
        }

        private Dictionary<string, HashSet<object>> _PropertyElement;
        /// <summary>
        /// Свойства элемента
        /// </summary>
        ///

        public override Dictionary<string, HashSet<object>> PropertyElement
        {
            get
            {
                if (_PropertyElement == null)
                {
                    _PropertyElement = new Dictionary<string, HashSet<object>>();
                }
                return _PropertyElement;
            }
            protected set
            {
                _PropertyElement = value;
            }
        }

        //public ObservableCollection<BasePropertySetDefinition> _CollectionPropertySet;

        ///// <summary>
        ///// Наборы характеристик
        ///// </summary>
        public ObservableCollection<BasePropertySetDefinition> CollectionPropertySet => new ObservableCollection<BasePropertySetDefinition>(ModelObjectEditor.FillCollectionPropertySet());
        //{
            //get
            //{
            //    if (_CollectionPropertySet is null)
            //    {
            //        _CollectionPropertySet = modelEditor.FillCollectionPropertySet();
            //    }
            //    return _CollectionPropertySet;
            //}
        //}

        private ObservableCollection<BaseModelItemIFC> _ModelItems;

        /// <summary>
        /// Элемент дерева
        /// </summary>
        public override ObservableCollection<BaseModelItemIFC> ModelItems
        {
            get
            {
                if (_ModelItems == null)
                {
                    _ModelItems = new ObservableCollection<BaseModelItemIFC>();
                }
                return _ModelItems;
            }
        }

        #endregion Свойства
    }

}