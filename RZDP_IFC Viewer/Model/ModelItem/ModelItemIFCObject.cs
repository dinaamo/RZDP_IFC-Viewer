using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Input;
using Editor_IFC;
using IFC_Table_View.IFC.Model;
using IFC_Table_View.Infracrucrure;
using IFC_Table_View.Infracrucrure.Commands;
using IFC_Table_View.Infracrucrure.FindObjectException;
using IFC_Table_View.View.Windows;
using IFC_Viewer.IFC.Base;
using IFC_Viewer.View.Windows;
using RZDP_IFC_Viewer.View.Windows;
using Xbim.Ifc4.Interfaces;

namespace IFC_Table_View.IFC.ModelItem
{
    public class ModelItemIFCObject : BaseModelItemIFC
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public ModelItemIFCObject(IIfcObjectDefinition IFCObject, ModelItemIFCObject TopElement, ModelIFC modelIFC) : base(modelIFC, IFCObject, TopElement)
        {
            //Если есть элемент выше по дереву то подключаем к нему обработчик события изменения состояния элемента
            if (TopElement != null)
            {
                PropertyReferenceChanged += TopElement.ChangePropertyReference;
            }

            IFCObjectDefinition = IFCObject;

            ZoomElementsCommand = new ActionCommand(
                   OnZoomElementsCommandExecuted,
                   CanZoomElementsCommandExecute);

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

            InitializationModelObject();
        }

        #region Комманды

        #region Показать элемент

        public ICommand ZoomElementsCommand { get; }

        private void OnZoomElementsCommandExecuted(object o)
        {
            Model.ZoomObject(this);
        }

        private bool CanZoomElementsCommandExecute(object o)
        {
            return true;
        }

        #endregion Показать элемент

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
                SearchAndEditWindow.CreateWindowSearch(SelectionElements(modelItem));
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
                SelectionElements(modelItem).ForEach(it => it.IsPaint = false);
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

                SelectReferenceObjectWindow window_Add_Reference_To_Table = new SelectReferenceObjectWindow(new List<ModelItemIFCObject> { this }, collectionModelReference, Model.AddReferenceToTheObject);

                window_Add_Reference_To_Table.ShowDialog();
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

            SelectReferenceObjectWindow window_Add_Reference_To_Table = new SelectReferenceObjectWindow(new List<ModelItemIFCObject> { this }, collectionModelReference, Model.DeleteReferenceToTheObject);

            window_Add_Reference_To_Table.ShowDialog();
        }

        private bool CanDeleteReferenceToTheTable(object o)
        {
            return true;
        }

        #endregion Удалить ссылки на документ или таблицу

        #endregion Комманды

        #region Методы

        #region Выделить элемент

        public void SelectElements()
        {
            Model.SelectObject(this);
        }

        #endregion Выделить элемент

        public void DeletePropertySet(BasePropertySetDefinition PropertySet)
        {
            Model.DeleteIFCEntity(PropertySet.IFCPropertySetDefinition);
            OnPropertyChanged("CollectionPropertySet");
        }


        public IIfcObjectDefinition GetIFCObject()
        {
            return IFCObjectDefinition;
        }

        /// <summary>
        /// Инициализация элементов объекта модели
        /// </summary>
        private void InitializationModelObject()
        {
            modelEditor = BaseEditorItem.CreateEditor(this, ModelIFC, IFCObjectDefinition);

            //CollectionPropertySet = modelEditor.FillCollectionPropertySet();

            PropertyElement = modelEditor.GetPropertyObject();

            int? countPropRef = CollectionPropertySet?.SelectMany(it => it.PropertyCollection).
                                                                Select(it => it.Property).
                                                                OfType<IIfcPropertyReferenceValue>().Count();

            if (countPropRef > 0)
            {
                IsContainPropertyReference = true;
                PropertyReferenceChanged?.Invoke(this, new PropertyReferenceChangedEventArg(IsContainPropertyReference));
            }
       
        }

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
                bool searchResult = SelectionElements(this)
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
            else
            {
                IsContainPropertyReferenceDownTree = false;
            }

            PropertyReferenceChanged?.Invoke(this, new PropertyReferenceChangedEventArg(IsContainPropertyReferenceDownTree));
        }

        private BaseEditorItem modelEditor;

        /// <summary>
        /// Удаление ссылок
        /// </summary>
        /// <param name="CollectionModelitemReferenceToDelete"></param>
        public void DeleteReferenceToTheObject(List<BaseModelReferenceIFC> CollectionModelitemReferenceToDelete)
        {
            try
            {
                //Удаляем нужные параметры и наборы
                int countPropSetContainsReference = modelEditor.DeleteReferenceToTheObject(CollectionModelitemReferenceToDelete);

                if (countPropSetContainsReference == 0)
                {
                    //Если ссылок в элементе больше нет то ставим флаг, что свойств нет
                    IsContainPropertyReference = false;
                }

                //Прокидываем наверх по дереву состояние флага
                PropertyReferenceChanged?.Invoke(this, new PropertyReferenceChangedEventArg(IsContainPropertyReference));
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
            IsContainPropertyReference = modelEditor.AddReferenceToTheObject(modelReferenceSet);

            //Прокидываем наверх по дереву событие добавления ссылки
            PropertyReferenceChanged?.Invoke(this, new PropertyReferenceChangedEventArg(IsContainPropertyReference));
        }

        /// <summary>
        /// Ищем все элементы в дереве по критериям
        /// </summary>
        /// <param name="topElement"></param>
        /// <param name="foundObjects"></param>
        public static void FindMultiplyTreeObject(ModelItemIFCObject topElement, IEnumerable<ModelItemIFCObject> foundObjects)
        {
            if (foundObjects.Any(it => it == topElement))
            {
                topElement.IsPaint = true;
            }

            topElement.IsExpanded = true;

            foreach (ModelItemIFCObject item in topElement.ModelItems)
            {
                FindMultiplyTreeObject(item, foundObjects);
            }
        }

        /// <summary>
        /// Ищем все элементы в дереве по критериям
        /// </summary>
        /// <param name="topElement"></param>
        /// <param name="foundObjects"></param>
        public static List<ModelItemIFCObject> FindPaintObjects(ModelItemIFCObject topElement)
        {
            return topElement.SelectionElements(topElement).Where(it => it.IsPaint).ToList();
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
        private List<ModelItemIFCObject> SelectionElements(ModelItemIFCObject modelItem)
        {
            List<ModelItemIFCObject> list = new List<ModelItemIFCObject>
            {
                modelItem
            };

            foreach (ModelItemIFCObject nestModelItem in modelItem.ModelItems)
            {
                list.AddRange(SelectionElements(nestModelItem));
            }

            return list;
        }

        #endregion Методы

        #region Свойства

        private IIfcObjectDefinition IFCObjectDefinition;

        public string IFCObjectGUID => IFCObjectDefinition.GlobalId.ToString();

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
        public ObservableCollection<BasePropertySetDefinition> CollectionPropertySet => new ObservableCollection<BasePropertySetDefinition>(modelEditor.FillCollectionPropertySet());
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