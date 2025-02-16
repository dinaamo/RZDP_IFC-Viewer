using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RZDP_IFC_Viewer.IFC.ModelItem;
using RZDP_IFC_Viewer.Infracrucrure.Commands;
using RZDP_IFC_Viewer.View.Windows.GroupOperation_Windows;
using RZDP_IFC_Viewer.ViewModels.Base;
using RZDP_IFC_Viewer.ViewModels.GroupOperation_Windows;
using Tedd;
using Xbim.Ifc4.Interfaces;

namespace RZDP_IFC_Viewer.ViewModels
{
    internal class SearchAndEditWindowViewModel : BaseViewModel
    {
        public string[] СonditionsSearch { get; private set; } = { "Равно", "Не равно", "Содержит", "Не содержит" };
        public List<ModelItemIFCObject> SearchItems { get; private set; }

        private ObservableCollection<ModelItemIFCObject> _FilteredSearchItems;

        public ObservableCollection<ModelItemIFCObject> FilteredSearchItems
        {
            get
            {
                return _FilteredSearchItems;
            }

            private set
            {
                _FilteredSearchItems = value;
                OnPropertyChanged("FilteredSearchItems");
            }
        }

        #region Комманды 

        #region Удалить элементы

        public ICommand DeleteModelObjectsCommand { get; }

        private void OnDeleteModelObjectsCommandExecuted(object o)
        {
            if (o is IEnumerable enumerable)
            {
                HashSet<ModelItemIFCObject> modelObjectsSet = new HashSet<ModelItemIFCObject>();

                foreach (ModelItemIFCObject modelObject in enumerable)
                {
                    modelObjectsSet.AddRange(ModelItemIFCObject.SelectionNestedItems(modelObject));
                }

                if (modelObjectsSet.Count() == 0 )
                {
                    return;
                }
                if (modelObjectsSet.Any(it => it.GetIFCObjectDefinition() is IIfcProject))
                {
                    MessageBox.Show($"Нельзя удалить IfcProject\n ", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                MessageBoxResult result = MessageBox.Show($"Удалить элементы?\n" +
                        $"Будет удалено элементов: {modelObjectsSet.Count}.", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                { return; }

                FilteredSearchItems[0].Model.DeleteModelObjects(modelObjectsSet);

                foreach (ModelItemIFCObject modelItemIFCObject in modelObjectsSet)
                {
                    SearchItems.Remove(modelItemIFCObject);
                }

                FilteredSearchItems = new(SearchItems);
            }
        }

        private bool CanDeleteModelObjectsCommandExecute(object o)
        {
            return FilteredSearchItems != null && FilteredSearchItems.Count() > 0;
        }

        #endregion Открыть групповой редактор наборов

        #region Открыть групповой редактор наборов характеристик

        public ICommand OpenGroupEditorPropertySetCommand { get; }

        private void OnOpenGroupEditorPropertySetCommandExecuted(object o)
        {
            if(o is IEnumerable enumerable)
            {
                HashSet<ModelItemIFCObject> modelObjectsSet =  new HashSet<ModelItemIFCObject>();

                foreach (ModelItemIFCObject modelObject in enumerable)
                {
                    modelObjectsSet.Add(modelObject);
                }
                new GroupEditPropertySetWindow(modelObjectsSet).ShowDialog();
            }
        }

        private bool CanOpenGroupEditorPropertySetCommandExecute(object o)
        {
            return FilteredSearchItems != null && FilteredSearchItems.Count() > 0;
        }

        #endregion Открыть групповой редактор наборов

        #region Открыть групповой редактор параметров
        
        public ICommand OpenGroupEditorPropertiesCommand { get; }

        private void OnOpenGroupEditorPropertiesCommandExecuted(object o)
        {
            if (o is IEnumerable enumerable)
            {
                HashSet<ModelItemIFCObject> modelObjectsSet = new HashSet<ModelItemIFCObject>();

                foreach (ModelItemIFCObject modelObject in enumerable)
                {
                    modelObjectsSet.Add(modelObject);
                }
                new GroupEditPropertyWindow(modelObjectsSet).ShowDialog();
            }
        }

        private bool CanOpenGroupEditorPropertiesCommandExecute(object o)
        {
            return FilteredSearchItems != null && FilteredSearchItems.Count() > 0;
        }

        #endregion Открыть групповой редактор параметров

        #region Открыть окно бодавления наборов по БД

        public ICommand OpenGroupAddPropSetFromDBWindowCommand { get; }

        private void OnGroupAddPropSetFromDBWindowCommandExecuted(object o)
        {
            if (o is IEnumerable enumerable)
            {
                HashSet<ModelItemIFCObject> modelObjectsSet = new HashSet<ModelItemIFCObject>();

                foreach (ModelItemIFCObject modelObject in enumerable)
                {
                    modelObjectsSet.Add(modelObject);
                }
                new GroupAddPropSetFromDBWindow(modelObjectsSet).ShowDialog();
            }
        }

        private bool CanGroupAddPropSetFromDBWindowCommandExecute(object o)
        {
            return FilteredSearchItems != null && FilteredSearchItems.Count() > 0;
        }

        #endregion Открыть окно бодавления наборов по БД

        #region Открыть групповой редактор имен объектов

        public ICommand OpenRenameModelObjectsWindowCommand { get; }

        private void OnOpenRenameModelObjectsWindowCommandExecuted(object o)
        {
            if (o is IEnumerable enumerable)
            {
                HashSet<ModelItemIFCObject> modelObjectsSet = new HashSet<ModelItemIFCObject>();

                foreach (ModelItemIFCObject modelObject in enumerable)
                {
                    modelObjectsSet.Add(modelObject);
                }
                new GroupRenameModelObjectsWindow(modelObjectsSet).ShowDialog();
            }
        }

        private bool CanOpenRenameModelObjectsWindowCommandExecute(object o)
        {
            return FilteredSearchItems != null && FilteredSearchItems.Count() > 0;
        }

        #endregion Открыть групповой редактор имен объектов

        #region Покрасить элементы

        public ICommand PaintElements { get; }

        private void OnPaintElementCommandExecuted(object o)
        {
            var topElement = SearchItems[0];
            var foundItems = FilteredSearchItems;
            //ModelItemIFCObject.FindMultiplyTreeObject(topElement, foundItems);

            ModelItemIFCObject.SelectionNestedItems(SearchItems[0]).
               Where(it => foundItems.Contains(it)).
               ToList().ForEach(it => { it.IsPaint = true; it.ExpandOver(); });
        }

        private bool CanPaintElementCommandExecute(object o)
        {
            return FilteredSearchItems != null && FilteredSearchItems.Count() > 0;
        }

        #endregion Покрасить элементы

        #region Сбросить условия поиска

        public ICommand ResetSeachСonditions { get; }

        private void OnResetSeachСonditionsCommandExecuted(object o)
        {
            //object[] ControlArray = (object[])o;

            //((ComboBox)ControlArray[0]).SelectedIndex = 2;
            //((ComboBox)ControlArray[1]).Text = string.Empty;

            //((ComboBox)ControlArray[2]).SelectedIndex = 2;
            //((ComboBox)ControlArray[3]).Text = string.Empty;

            //((ComboBox)ControlArray[4]).SelectedIndex = 2;
            //((ComboBox)ControlArray[5]).Text = string.Empty;

            //((ComboBox)ControlArray[6]).SelectedIndex = 2;
            //((ComboBox)ControlArray[7]).Text = string.Empty;

            //((ComboBox)ControlArray[8]).SelectedIndex = 2;
            //((ComboBox)ControlArray[9]).Text = string.Empty;

            //((ComboBox)ControlArray[10]).SelectedIndex = 2;
            //((ComboBox)ControlArray[11]).Text = string.Empty;

            //DataGrid dataGrid = ControlArray[12] as DataGrid;

            FilteredSearchItems = new ObservableCollection<ModelItemIFCObject>(SearchItems);
            //dataGrid.ItemsSource = FilteredSearchItems;
        }

        private bool CanResetSearchСonditionsCommandExecute(object o)
        {
            return true;
        }

        #endregion Сбросить условия поиска

        #region Фильтр

        public ICommand FilteredElements { get; }

        private void OnFilteredElementsCommandExecuted(object o)
        {
            object[] ControlArray = (object[])o;

            string FilterSearchValueGUID = (string)ControlArray[0];
            string textGUID = (string)ControlArray[1];

            string FilterSearchValueClassElement = (string)ControlArray[2];
            string textClassElement = (string)ControlArray[3];

            string FilterSearchValueNameElement = (string)ControlArray[4];
            string textNameElement = (string)ControlArray[5];

            string FilterSearchValuePropertySet = (string)ControlArray[6];
            string textPropertySet = (string)ControlArray[7];

            string FilterSearchValuePropertyName = (string)ControlArray[8];
            string textPropertyName = (string)ControlArray[9];

            string FilterSearchValuePropertyValue = (string)ControlArray[10];
            string textPropertyValue = (string)ControlArray[11];

            //DataGrid dataGrid = ControlArray[12] as DataGrid;

            //dataGrid.ItemsSource = null;


            var col1 = SearchItems.Where(it => IsFilterString(new List<string>() { it.IFCObjectGUID }, textGUID, FilterSearchValueGUID));
            var col2 = col1.Where(it => IsFilterString(new List<string>() { it.IFCClass }, textClassElement, FilterSearchValueClassElement));
            var col3 = col2.Where(it => IsFilterString(new List<string>() { it.IFCObjectName }, textNameElement, FilterSearchValueNameElement));
            var col4 = col3.Where(it => IsFilterString(it.CollectionPropertySet.Select(it => it.NamePropertySet), textPropertySet, FilterSearchValuePropertySet));
            var col5 = col4.Where(it => IsFilterString(it.CollectionPropertySet.SelectMany(it => it.PropertyCollection).Select(it => it.NameProperty), textPropertyName, FilterSearchValuePropertyName));
            var col6 = col5.Where(it => IsFilterString(it.CollectionPropertySet.SelectMany(it => it.PropertyCollection).Select(it => it.ValueString), textPropertyValue, FilterSearchValuePropertyValue));

            FilteredSearchItems = new ObservableCollection<ModelItemIFCObject>(col6);

            //dataGrid.ItemsSource = FilteredSearchItems;
        }

        private bool IsFilterString(IEnumerable<string> stringCollection, string seachString, string seachingFilter)
        {
            if (seachString == string.Empty)
            {
                return true;
            }
            else
            {
                if (seachingFilter == "Равно")
                {
                    return stringCollection.Any(str => str.Equals(seachString));
                }
                else if (seachingFilter == "Не равно")
                {
                    return !stringCollection.Any(str => str.Equals(seachString));
                }
                else if (seachingFilter == "Содержит")
                {
                    return stringCollection.Any(str => str.Contains(seachString));
                }
                else if (seachingFilter == "Не содержит")
                {
                    return !stringCollection.Any(str => str.Contains(seachString));
                }
                else
                {
                    return false;
                }
            }
        }

        private bool CanFilteredElementsCommandExecute(object o)
        {
            return true;
        }

        #endregion Фильтр

        #endregion Комманды

        public SearchAndEditWindowViewModel()
        {
        }

        public SearchAndEditWindowViewModel(IEnumerable<ModelItemIFCObject> modelElementsForSearch)
        {
            SearchItems = new(modelElementsForSearch);

            FilteredSearchItems = new ObservableCollection<ModelItemIFCObject>(modelElementsForSearch);

            #region Комманды

            DeleteModelObjectsCommand = new ActionCommand(
                OnDeleteModelObjectsCommandExecuted,
                CanDeleteModelObjectsCommandExecute);

            OpenGroupEditorPropertySetCommand = new ActionCommand(
                OnOpenGroupEditorPropertySetCommandExecuted,
                CanOpenGroupEditorPropertySetCommandExecute);

            OpenGroupEditorPropertiesCommand = new ActionCommand(
                OnOpenGroupEditorPropertiesCommandExecuted,
                CanOpenGroupEditorPropertiesCommandExecute);

            OpenGroupAddPropSetFromDBWindowCommand = new ActionCommand(
                OnGroupAddPropSetFromDBWindowCommandExecuted,
                CanGroupAddPropSetFromDBWindowCommandExecute);

            OpenRenameModelObjectsWindowCommand = new ActionCommand(
                OnOpenRenameModelObjectsWindowCommandExecuted,
                CanOpenRenameModelObjectsWindowCommandExecute);

            PaintElements = new ActionCommand(
                OnPaintElementCommandExecuted,
                CanPaintElementCommandExecute);

            FilteredElements = new ActionCommand(
                OnFilteredElementsCommandExecuted,
                CanFilteredElementsCommandExecute);

            ResetSeachСonditions = new ActionCommand(
                OnResetSeachСonditionsCommandExecuted,
                CanResetSearchСonditionsCommandExecute);

            #endregion Комманды
        }
    }
}