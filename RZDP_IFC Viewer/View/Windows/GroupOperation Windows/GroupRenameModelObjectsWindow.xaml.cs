using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using RZDP_IFC_Viewer.IFC.ModelItem;
using RZDP_IFC_Viewer.Infracrucrure;
using RZDP_IFC_Viewer.ViewModels;

namespace RZDP_IFC_Viewer.View.Windows.GroupOperation_Windows
{
    /// <summary>
    /// Логика взаимодействия для GroupRenameModelObjectsWindow.xaml
    /// </summary>
    public partial class GroupRenameModelObjectsWindow : Window
    {
        public GroupRenameModelObjectsWindow(IEnumerable<ModelItemIFCObject> FilteredSearchItems)
        {
            InitializeComponent();
            DataContext = new GroupRenameModelObjectsViewModel(FilteredSearchItems);
        }

        private void tbSearchingValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((GroupRenameModelObjectsViewModel)DataContext).Search(tbSearchingValue.Text);
        }

        private void Button_Click_Clear(object sender, RoutedEventArgs e)
        {
            ((GroupRenameModelObjectsViewModel)DataContext).ResetSearchConditions();
            tbSearchingValue.Text = string.Empty;
            tbSetValue.Text = string.Empty;
        }


        //private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        //{
        //    UpdateColumnsWidth(sender as ListView);
        //}

        //private void ListView_Loaded(object sender, RoutedEventArgs e)
        //{
        //    UpdateColumnsWidth(sender as ListView);
        //}

        //private void UpdateColumnsWidth(ListView listView)
        //{
        //    HelperWPF.UpdateColumnsWidth(listView);
        //}

    }
}
