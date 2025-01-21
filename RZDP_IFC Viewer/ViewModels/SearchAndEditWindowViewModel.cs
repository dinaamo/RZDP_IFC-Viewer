using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using IFC_Table_View.IFC.ModelItem;
using IFC_Table_View.Infracrucrure.Commands;
using IFC_Table_View.ViewModels.Base;

namespace IFC_Table_View.ViewModels
{
    internal class SearchAndEditWindowViewModel : BaseViewModel
    {
        public string[] СonditionsSearch { get; private set; } = { "Равно", "Не равно", "Содержит", "Не содержит" };
        public ObservableCollection<ModelItemIFCObject> SearchItems { get; private set; }

        private ObservableCollection<ModelItemIFCObject> _FilteredSearchItems;

        public ObservableCollection<ModelItemIFCObject> FilteredSearchItems
        {
            get
            {
                return _FilteredSearchItems;
            }

            private set
            {
                OnPropertyChanged("FilteredSearchItems");
                _FilteredSearchItems = value;
            }
        }

        #region Комманды

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
            object[] ControlArray = (object[])o;

            ((ComboBox)ControlArray[0]).SelectedIndex = 2;
            ((ComboBox)ControlArray[1]).Text = string.Empty;

            ((ComboBox)ControlArray[2]).SelectedIndex = 2;
            ((ComboBox)ControlArray[3]).Text = string.Empty;

            ((ComboBox)ControlArray[4]).SelectedIndex = 2;
            ((ComboBox)ControlArray[5]).Text = string.Empty;

            ((ComboBox)ControlArray[6]).SelectedIndex = 2;
            ((ComboBox)ControlArray[7]).Text = string.Empty;

            ((ComboBox)ControlArray[8]).SelectedIndex = 2;
            ((ComboBox)ControlArray[9]).Text = string.Empty;

            ((ComboBox)ControlArray[10]).SelectedIndex = 2;
            ((ComboBox)ControlArray[11]).Text = string.Empty;

            DataGrid dataGrid = ControlArray[12] as DataGrid;

            FilteredSearchItems = new ObservableCollection<ModelItemIFCObject>(SearchItems);
            dataGrid.ItemsSource = FilteredSearchItems;
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

            string FilterSearchValueGUID = ((ComboBox)ControlArray[0]).Text;
            string textGUID = ((ComboBox)ControlArray[1]).Text;

            string FilterSearchValueClassElement = ((ComboBox)ControlArray[2]).Text;
            string textClassElement = ((ComboBox)ControlArray[3]).Text;

            string FilterSearchValueNameElement = ((ComboBox)ControlArray[4]).Text;
            string textNameElement = ((ComboBox)ControlArray[5]).Text;

            string FilterSearchValuePropertySet = ((ComboBox)ControlArray[6]).Text;
            string textPropertySet = ((ComboBox)ControlArray[7]).Text;

            string FilterSearchValuePropertyName = ((ComboBox)ControlArray[8]).Text;
            string textPropertyName = ((ComboBox)ControlArray[9]).Text;

            string FilterSearchValuePropertyValue = ((ComboBox)ControlArray[10]).Text;
            string textPropertyValue = ((ComboBox)ControlArray[11]).Text;

            DataGrid dataGrid = ControlArray[12] as DataGrid;

            dataGrid.ItemsSource = null;


            var col1 = SearchItems.Where(it => IsFilterString(new List<string>() { it.IFCObjectGUID }, textGUID, FilterSearchValueGUID));
            var col2 = col1.Where(it => IsFilterString(new List<string>() { it.IFCClass }, textClassElement, FilterSearchValueClassElement));
            var col3 = col2.Where(it => IsFilterString(new List<string>() { it.IFCObjectName }, textNameElement, FilterSearchValueNameElement));
            var col4 = col3.Where(it => IsFilterString(it.CollectionPropertySet.Select(it => it.NamePropertySet), textPropertySet, FilterSearchValuePropertySet));
            var col5 = col4.Where(it => IsFilterString(it.CollectionPropertySet.SelectMany(it => it.PropertyCollection).Select(it => it.NameProperty), textPropertyName, FilterSearchValuePropertyName));
            var col6 = col5.Where(it => IsFilterString(it.CollectionPropertySet.SelectMany(it => it.PropertyCollection).Select(it => it.ValueString), textPropertyValue, FilterSearchValuePropertyValue));

            FilteredSearchItems = new ObservableCollection<ModelItemIFCObject>(col6);

            dataGrid.ItemsSource = FilteredSearchItems;
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
            SearchItems = new ObservableCollection<ModelItemIFCObject>(modelElementsForSearch);

            FilteredSearchItems = new ObservableCollection<ModelItemIFCObject>(modelElementsForSearch);

            #region Комманды

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