using System.CodeDom;
using System.Windows;
using RZDP_IFC_Viewer.IFC.ModelItem;
using RZDP_IFC_Viewer.ViewModels;

namespace RZDP_IFC_Viewer.View.Windows
{
    /// <summary>
    /// Логика взаимодействия для SearchWindow.xaml
    /// </summary>
    public partial class SearchAndEditWindow : Window
    {
        private static SearchAndEditWindow instance;

        public static void CreateWindowSearch(IEnumerable<ModelItemIFCObject> ModelElementsForSearch)
        {
            if (instance == null)
            {
                instance = new SearchAndEditWindow(ModelElementsForSearch);
                instance.Show();
                instance.Topmost = true;
                instance.Topmost = false;
            }
            else
            {
                instance.WindowState = WindowState.Normal;
                instance.Topmost = true;
                instance.Topmost = false;
                return;
            }
        }

        IEnumerable<ModelItemIFCObject> _modelElementsForSearch;

        private SearchAndEditWindow(IEnumerable<ModelItemIFCObject> ModelElementsForSearch)
        {
            InitializeComponent();

            //dgSearchElements.EnableColumnVirtualization = true;
            //dgSearchElements.EnableRowVirtualization = true;

            

            _modelElementsForSearch = ModelElementsForSearch;

            SearchAndEditWindowViewModel searchWindowViewModel = new SearchAndEditWindowViewModel(ModelElementsForSearch);

            DataContext = searchWindowViewModel;

            CBGUIDValue.ItemsSource = searchWindowViewModel.SearchItems.Select(it => it.IFCObjectGUID).Distinct();
            CBClassElementValue.ItemsSource = searchWindowViewModel.SearchItems.Select(it => it.IFCClass).Distinct();
            CBNameElementValue.ItemsSource = searchWindowViewModel.SearchItems.Select(it => it.IFCObjectName).Distinct();
            CBPropertySetValue.ItemsSource = searchWindowViewModel.SearchItems.SelectMany(it => it.CollectionPropertySet).Select(it => it.NamePropertySet).Distinct();
            CBPropertyNameValue.ItemsSource = searchWindowViewModel.SearchItems.SelectMany(it => it.CollectionPropertySet).SelectMany(it => it.PropertyCollection).Select(it => it.NameProperty).Distinct();
            CBPropertyValue.ItemsSource = searchWindowViewModel.SearchItems.SelectMany(it => it.CollectionPropertySet).SelectMany(it => it.PropertyCollection).Select(it => it.ValueString).Distinct();
        }

        private void OnComboboxTextChanged(object sender, RoutedEventArgs e)
        {
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            instance = null;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            foreach (var modelObject in _modelElementsForSearch)
            {
                modelObject.OnPropertyChanged("CollectionPropertySet");
            }
        }

        private void dgSearchElements_CurrentCellChanged(object sender, EventArgs e)
        {
            (lwSearchElements?.SelectedItem as ModelItemIFCObject)?.OnPropertyChanged("CollectionPropertySet");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CBGUIDFilter.SelectedIndex = 2;
            CBGUIDValue.Text = string.Empty;
            CBClassElementFilter.SelectedIndex = 2;
            CBClassElementValue.Text = string.Empty;
            CBNameElementFilter.SelectedIndex = 2;
            CBNameElementValue.Text = string.Empty;
            CBPropertySetFilter.SelectedIndex = 2;
            CBPropertySetValue.Text = string.Empty;
            CBPropertyNameFilter.SelectedIndex = 2;
            CBPropertyNameValue.Text = string.Empty;
            CBPropertyValueFilter.SelectedIndex = 2;
            CBPropertyValue.Text = string.Empty;
        }
    }
}