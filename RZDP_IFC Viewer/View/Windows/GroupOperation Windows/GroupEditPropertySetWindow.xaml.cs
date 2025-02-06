using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Editor_IFC;
using RZDP_IFC_Viewer.IFC.ModelItem;
using RZDP_IFC_Viewer.Infracrucrure;
using RZDP_IFC_Viewer.ViewModels;

namespace RZDP_IFC_Viewer.View.Windows.GroupOperation_Windows
{
    /// <summary>
    /// Логика взаимодействия для GroupEditPropertySetWindow.xaml
    /// </summary>
    public partial class GroupEditPropertySetWindow : Window
    {
        public GroupEditPropertySetWindow(IEnumerable<ModelItemIFCObject> FilteredSearchItems)
        {
            InitializeComponent();
            DataContext = new GroupEditPropertySetViewModel(FilteredSearchItems);
        }


        private void Button_Click_Search(object sender, RoutedEventArgs e)
        {
            ((GroupEditPropertySetViewModel)DataContext).Search(tbFindValue.Text);
        }

        private void Button_Click_Clear(object sender, RoutedEventArgs e)
        {
            ((GroupEditPropertySetViewModel)DataContext).ResetSearchConditions();
            tbFindValue.Text = string.Empty;
            tbSetValue.Text = string.Empty;
        }

        private void tbFindValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((GroupEditPropertySetViewModel)DataContext).Search(tbFindValue.Text);
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
