using System.CodeDom;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Editor_IFC;
using RZDP_IFC_Viewer;
using RZDP_IFC_Viewer.IFC.ModelItem;
using PropertyTools.Wpf;
using RZDP_IFC_Viewer.IFC.Model.ModelObjectPropertySet.Base;
using Xbim.Ifc4.Interfaces;
using RZDP_IFC_Viewer.ViewModels;

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
        }



        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                
            }
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
            //Получаем контекст ячейки datagrid
            MenuItem menuItem = sender as MenuItem;
            //Если набор
            if (menuItem?.DataContext is BasePropertySetDefinition propertySetDefinitionModel) 
            {
                DeleteProperySet(propertySetDefinitionModel);
            }
            //Если свойство
            else if (menuItem.DataContext is IPropertyModel<IIfcResourceObjectSelect> propertyModel) 
            {
                DeleteProperty(propertyModel);
            }
        }

        void DeleteProperySet(BasePropertySetDefinition propertySetDefinitionModel)
        {
            if (propertySetDefinitionModel.IFCPropertySetDefinition is IIfcPropertySet ifcPropertySet)
            {
                if (ifcPropertySet.HasProperties.Any(pr => pr is IIfcPropertyReferenceValue))
                {
                    MessageBoxResult result = MessageBox.Show("Удалить набор характеристик с ссылками?\n" +
                        "Удаление следует производить через панель инструментов", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.No)
                    { return; }
                }
            }
            if (propertySetDefinitionModel.CountRelatedObjectsInstanse > 1 || propertySetDefinitionModel.CountRelatedObjectsType > 1)
            {
                MessageBoxResult result = MessageBox.Show("На данный набор характеристик ссылается более одного объекта?\n" +
                        "Продолжить?", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                { return; }
            }
                 //Удаляем
                 (DataContext as ModelItemIFCObject)?.DeletePropertySet(propertySetDefinitionModel.IFCPropertySetDefinition);
        }

        void DeleteProperty(IPropertyModel<IIfcResourceObjectSelect> propertyModel)
        {
            if (propertyModel.Property is IIfcPropertyReferenceValue ifcProperty)
            {
                MessageBoxResult result = MessageBox.Show("Удалить ссылку?\n" +
                    "Удаление следует производить через панель инструментов.", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                { return; }
            }
            //Удаляем
            propertyModel.PropertySetDefinition.DeletePropertyModel(propertyModel);
        }


        private void MenuItemDublicate_Click(object sender, RoutedEventArgs e)
        {
            //Получаем контекст ячейки datagrid
            MenuItem menuItem = sender as MenuItem;
            //Если набор
            if (menuItem?.DataContext is BasePropertySetDefinition propertySetDefinitionModel)
            {
                //Создаем дубликат
                 (DataContext as ModelItemIFCObject)?.AddDublicatePropertySet(propertySetDefinitionModel.IFCPropertySetDefinition);
            }
        }


        private void MenuItemUnpin_Click(object sender, RoutedEventArgs e)
        {
            //Получаем контекст ячейки datagrid
            MenuItem menuItem = sender as MenuItem;
            //Если набор
            if (menuItem?.DataContext is BasePropertySetDefinition propertySetDefinitionModel)
            {
                //Открепляем
                 (DataContext as ModelItemIFCObject)?.UnpinPropertySet(propertySetDefinitionModel.IFCPropertySetDefinition);
            }
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                //dgPropertySet.SelectedIndex = 0;
                if (dgPropertySet.Items.Count >0)
                {
                    dgPropertySet.CurrentCell = new DataGridCellInfo(dgPropertySet.Items[0], dgPropertySet.Columns[0]);
                    dgPropertySet.SelectedItem = dgPropertySet.CurrentCell;
                }
            });
        }
    }
}