using System.CodeDom;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Editor_IFC;
using IFC_Table_View.IFC.ModelItem;
using RZDP_IFC_Viewer.IFC.Model.ModelObjectPropertySet.Base;
using Xbim.Ifc4.Interfaces;

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
            //Получаем контекст ячейки datagrid
            MenuItem menuItem = sender as MenuItem;
            //Если набор
            if (menuItem.DataContext is BasePropertySetDefinition propertySetDefinitionModel) 
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
                //Удаляем
                 (DataContext as ModelItemIFCObject)?.DeletePropertySet(propertySetDefinitionModel);

            }
            //Если свойство
            else if (menuItem.DataContext is IPropertyModel<IIfcResourceObjectSelect> propertyModel) 
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
        }
    }
}