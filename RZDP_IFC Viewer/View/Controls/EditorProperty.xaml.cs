using System.CodeDom;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Editor_IFC;
using IFC_Table_View.IFC.ModelItem;

namespace RZDP_IFC_Viewer.View.Controls
{
    /// <summary>
    /// Логика взаимодействия для EditorProperty.xaml
    /// </summary>
    public partial class EditorProperty : UserControl
    {
        public EditorProperty()
        {
            InitializeComponent();
            DataContextChanged += EditorProperty_DataContextChanged;
        }

        private void EditorProperty_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //Dispatcher.BeginInvoke(
            //    new Action(() => dgPropertySet.SelectedIndex = 0), DispatcherPriority.DataBind);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var tt = dgPropertySet;
            var t4t = dgProperty;
        }
        
        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBox tb = sender as TextBox;
            tb.Focusable = true;
            tb.Background = Brushes.LightCyan;
            tb.Focus();
        }

        private void tbName_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            tb.Focusable = false;
            tb.Background = null;
        }

        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;

            if (menuItem.DataContext is BasePropertySetDefinition PropertySetDefinition)
            {
                if (((ModelItemIFCObject)DataContext).DeletePropertySet(PropertySetDefinition))
                {
                    List<BasePropertySetDefinition> tempCollection = ((List<BasePropertySetDefinition>)(dgPropertySet.ItemsSource)).Where(it => !it.Equals(PropertySetDefinition)).ToList();
                    dgPropertySet.ItemsSource = null;
                    dgPropertySet.ItemsSource = tempCollection;
                } 
            }
        }
    }
}