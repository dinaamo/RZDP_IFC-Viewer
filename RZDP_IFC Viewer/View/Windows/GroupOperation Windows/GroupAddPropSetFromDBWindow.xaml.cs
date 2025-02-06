using System;
using System.Collections.Generic;
using System.Data;
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

namespace RZDP_IFC_Viewer.ViewModels.GroupOperation_Windows
{
    /// <summary>
    /// Логика взаимодействия для GroupEditNameWindow.xaml
    /// </summary>
    public partial class GroupAddPropSetFromDBWindow : Window
    {
        public GroupAddPropSetFromDBWindow()
        {
            InitializeComponent();
        }

        GroupAddPropSetFromDBViewModel _groupAddPropSetFromDBViewModel;

        public GroupAddPropSetFromDBWindow(IEnumerable<ModelItemIFCObject> FilteredSearchItems)
        {
            InitializeComponent();
            _groupAddPropSetFromDBViewModel = new GroupAddPropSetFromDBViewModel(FilteredSearchItems);
            DataContext = _groupAddPropSetFromDBViewModel;
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
