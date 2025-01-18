using System.Windows;
using IFC_Table_View.IFC.ModelItem;
using IFC_Table_View.ViewModels;

namespace IFC_Table_View.View.Windows
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
            }
            else
            {
                return;
            }
        }

        private SearchAndEditWindow(IEnumerable<ModelItemIFCObject> ModelElementsForSearch)
        {
            InitializeComponent();

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
    }
}