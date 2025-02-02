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
using Editor_IFC;
using RZDP_IFC_Viewer.IFC.Model.ModelObjectPropertySet.Base;
using RZDP_IFC_Viewer.IFC.ModelItem;
using RZDP_IFC_Viewer.ViewModels;
using Xbim.Ifc4.Interfaces;

namespace RZDP_IFC_Viewer.View.Windows.GroupOperation_Windows
{
    /// <summary>
    /// Логика взаимодействия для GroupEditPropertyWindow.xaml
    /// </summary>
    public partial class GroupEditPropertyWindow : Window
    {
        public GroupEditPropertyWindow(IEnumerable<ModelItemIFCObject> FilteredSearchItems)
        {
            InitializeComponent();
            DataContext = new GroupEditPropertyViewModel(FilteredSearchItems);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            foreach (IPropertyModel<IIfcResourceObjectSelect> propertySet in dgProperties.ItemsSource)
            {
                //propertySet.PropertySetDefinition.ModelObject.OnPropertyChanged("CollectionPropertySet");
            }
        }

        private void Button_Click_Search(object sender, RoutedEventArgs e)
        {
            ((GroupEditPropertyViewModel)DataContext).Search(tbSearchingName.Text, tbSearchingValue.Text);
        }

        private void Button_Click_Clear(object sender, RoutedEventArgs e)
        {
            ((GroupEditPropertyViewModel)DataContext).ResetSearchConditions();
            tbSearchingName.Text = string.Empty;
            tbSetName.Text = string.Empty;
            tbSearchingValue.Text = string.Empty;
            tbSetValue.Text = string.Empty;
        }
    }
}
