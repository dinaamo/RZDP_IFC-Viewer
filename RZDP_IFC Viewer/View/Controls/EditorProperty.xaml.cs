using System.CodeDom;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Editor_IFC;
using RZDP_IFC_Viewer;
using RZDP_IFC_Viewer.IFC.ModelItem;
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

            //Получаем контекст ячейки datagrid на которой вызвано меню
            MenuItem menuItem = sender as MenuItem;

            //Если набор свойств
            if (menuItem?.DataContext is BasePropertySetDefinition propertySetDefinitionModel)
            {
                List<BasePropertySetDefinition> deleteElements = new List<BasePropertySetDefinition>();

                foreach (BasePropertySetDefinition item in dgPropertySet.SelectedItems)
                {
                    deleteElements.Add(item);
                }

                foreach (BasePropertySetDefinition deletePropertySet in deleteElements)
                {
                    DeleteProperySet(deletePropertySet);
                }
            }
            //Если свойство
            else if (menuItem.DataContext is IPropertyModel<IIfcResourceObjectSelect> propertyModel)
            {
                List<IPropertyModel<IIfcResourceObjectSelect>> deleteElements = new List<IPropertyModel<IIfcResourceObjectSelect>>();

                foreach (IPropertyModel<IIfcResourceObjectSelect> item in dgProperty.SelectedItems)
                {
                    deleteElements.Add(item);
                }

                foreach (IPropertyModel<IIfcResourceObjectSelect> deleteProperty in deleteElements)
                {
                    DeleteProperty(deleteProperty);
                }
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
            if (propertySetDefinitionModel.CountRelatedObjectsInstance > 1 || propertySetDefinitionModel.CountRelatedObjectsType > 1)
            {
                MessageBoxResult result = MessageBox.Show("На данный набор характеристик ссылается более одного объекта?\n" +
                        "Продолжить?", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                { return; }
            }
                 //Удаляем
                 (DataContext as ModelItemIFCObject)?.DeletePropertySet(propertySetDefinitionModel);
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
                List<BasePropertySetDefinition> duplicateElements = new List<BasePropertySetDefinition>();

                foreach (BasePropertySetDefinition item in dgPropertySet.SelectedItems)
                {
                    duplicateElements.Add(item);
                }

                //Создаем дубликат
                foreach (BasePropertySetDefinition duplicatePropertySet in duplicateElements)
                {
                    (DataContext as ModelItemIFCObject)?.AddDublicatePropertySet(duplicatePropertySet);
                }

            }
        }


        private void MenuItemUnpin_Click(object sender, RoutedEventArgs e)
        {
            //Получаем контекст ячейки datagrid
            MenuItem menuItem = sender as MenuItem;
            //Если набор
            if (menuItem?.DataContext is BasePropertySetDefinition propertySetDefinitionModel)
            {
                List<BasePropertySetDefinition> unpinElements = new List<BasePropertySetDefinition>();

                foreach (BasePropertySetDefinition item in dgPropertySet.SelectedItems)
                {
                    unpinElements.Add(item);
                }

                //Создаем дубликат
                foreach (BasePropertySetDefinition unpinPropertySet in unpinElements)
                {
                    //Открепляем
                    (DataContext as ModelItemIFCObject)?.UnpinPropertySet(unpinPropertySet);
                }
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

        private void Button_ExportParametersToXML_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as ModelItemIFCObject)?.ExportParametersToXML();
        }

        private void Button_ImportParametersToXML_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as ModelItemIFCObject)?.ImportParametersFromXML();
        }


    }
}