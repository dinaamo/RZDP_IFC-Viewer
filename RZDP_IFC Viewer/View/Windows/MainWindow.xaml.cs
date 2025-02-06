using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RZDP_IFC_Viewer.IFC.ModelItem;
using RZDP_IFC_Viewer.Infracrucrure.FindObjectException;
using RZDP_IFC_Viewer.View.Windows;
using RZDP_IFC_Viewer.ViewModels;
using RZDP_IFC_Viewer.IFC.Model.ModelObjectPropertySet.Base;
using Xbim.Ifc4.Interfaces;

namespace RZDP_IFC_Viewer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //public static bool IsReadOnly { get; private set; } = true;
        //public static bool IsEnabled { get; private set; } = false;
        //public static Visibility IsVisibility { get; private set; } = Visibility.Collapsed;

        public static bool IsReadOnly { get; private set; } = false;
        public static bool IsEnabled { get; private set; } = true;
        public static Visibility IsVisibility { get; private set; } = Visibility.Visible;



        public MainWindow()
        {
            InitializeComponent();
        }

        //IProgress<(double percente, string message)> progress;
        private void MainWindowIFC_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!viewModel.CloseApplication())
            {
                e.Cancel = true;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;

            if (menuItem.Name.Equals("CloseApplication"))
            {
                this.Close();
            }
        }

        private MainWindowViewModel viewModel;

        #region Дерево и свойства

        /// Двойной клик
        public void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                if (sender is TextBlock textBlock)
                {
                    if (textBlock.DataContext is IPropertyModel<IIfcProperty> ifcProperty)
                    {
                        if (ifcProperty.Property is IIfcPropertyReferenceValue ifcPropertyRefVa)
                        {
                            ObservableCollection<BaseModelItemIFC> collectionObjectModel = treeViewIFC.ItemsSource as ObservableCollection<BaseModelItemIFC>;

                            BaseModelReferenceIFC? targetBaseReferenceModel = collectionObjectModel[0].ModelItems.
                                                                    OfType<BaseModelReferenceIFC>().
                                                                    FirstOrDefault(it => it.GetReference().Equals(ifcPropertyRefVa?.PropertyReference));

                            if (targetBaseReferenceModel is ModelItemIFCTable modelIfcTable)
                            {
                                TableWindow.CreateTableWindow(modelIfcTable);
                            }
                            else if (targetBaseReferenceModel is ModelItemDocumentReference modelItemDocumentReference)
                            {
                                modelItemDocumentReference.OpenDocument();
                            }
                        }
                    }
                    else if (textBlock.DataContext is ModelItemIFCObject searchModelObject)
                    {
                        //ModelItemIFCObject findObj = FindModelObject(searchModelObject.ItemIFC);
                        //if (findObj != null)
                        //{
                        searchModelObject.ExpandOver();
                        searchModelObject.IsSelected = true;
                        searchModelObject.IsFocusReference = false;
                        //}
                    }
                }
            }
        }


        /// Загрузка формы
        private void MainWindowIFC_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel = new MainWindowViewModel(this);
            DataContext = viewModel;
        }

        private void MainWindowIFC_DragLeave(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                viewModel.LoadApplicationCommand.Execute(files[0]);
            }
        }

        //Обработка события потери фокуса мыши
        private void IsMouseLostFocus(object sender, MouseEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                if (textBlock.DataContext is ModelItemIFCObject findObject)
                {
                    findObject.IsFocusReference = false;
                }
                else if (textBlock.DataContext is IPropertyModel<IIfcProperty> ifcProperty)
                {
                    if (ifcProperty.Property is IIfcPropertyReferenceValue ifcPropertyRefVa)
                    {
                        IIfcPropertyReferenceValue PropertyRefVa = ifcPropertyRefVa;

                        ObservableCollection<BaseModelItemIFC> collectionObjectModel = treeViewIFC.ItemsSource as ObservableCollection<BaseModelItemIFC>;

                        BaseModelReferenceIFC targetBaseReferenceModel = collectionObjectModel[0].ModelItems.
                                                                        OfType<BaseModelReferenceIFC>().
                                                                        FirstOrDefault(it => it.ItemIFC.Equals(PropertyRefVa?.PropertyReference));

                        if (targetBaseReferenceModel != null)
                        {
                            targetBaseReferenceModel.IsFocusReference = false;
                        }
                    }
                }
            }
        }

        //Обработка события получения фокуса мыши
        private void IsMouseFocus(object sender, MouseEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                if (textBlock.DataContext is ModelItemIFCObject findObject)
                {
                    findObject.IsFocusReference = true;
                }
                else if (textBlock.DataContext is IPropertyModel<IIfcProperty> ifcProperty)
                {
                    if (ifcProperty.Property is IIfcPropertyReferenceValue ifcPropertyRefVa)
                    {
                        IIfcPropertyReferenceValue PropertyRefVa = ifcPropertyRefVa;

                        ObservableCollection<BaseModelItemIFC> collectionObjectModel = treeViewIFC.ItemsSource as ObservableCollection<BaseModelItemIFC>;

                        BaseModelReferenceIFC targetBaseReferenceModel = collectionObjectModel[0].ModelItems.
                                                                        OfType<BaseModelReferenceIFC>().
                                                                        FirstOrDefault(it => it.ItemIFC.Equals(PropertyRefVa?.PropertyReference));

                        if (targetBaseReferenceModel != null)
                        {
                            targetBaseReferenceModel.IsFocusReference = true;
                        }
                    }
                }
            }
        }

        //Установка стиля для колонок dataGrid
        private void SetColumnStyle()
        {
            Style columnStyle = new Style(typeof(TextBlock));
            columnStyle.Setters.Add(new Setter(
                                                TextBlock.TextWrappingProperty,
                                                TextWrapping.Wrap
                                                ));
            foreach (DataGridTextColumn column in dgTable.Columns)
            {
                column.ElementStyle = columnStyle;
            }
        }

        //Подбор содержимого колонок по контексту
        private void ResizeColumns()
        {
            foreach (var column in dgTable.Columns)
            {
                column.Width = DataGridLength.SizeToHeader;
                double sizeToHeader = column.Width.DesiredValue;

                column.Width = DataGridLength.SizeToCells;
                double sizeToCells = column.Width.DesiredValue;

                if (sizeToHeader > sizeToCells || sizeToCells < 50 || sizeToCells > 300)
                {
                    column.Width = DataGridLength.SizeToHeader;
                }
                else if (sizeToHeader < sizeToCells || sizeToHeader < 50 || sizeToHeader > 300)
                {
                    column.Width = DataGridLength.SizeToCells;
                }
                else
                {
                    column.Width = new DataGridLength(dgTable.Width / dgTable.Columns.Count, DataGridLengthUnitType.Auto);
                }
            }
        }

        private int countItems = 0;

        //Событие загрузка датагрид
        private void dgTable_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            DataGrid dataGrid = (DataGrid)sender;

            if (countItems < dataGrid.Items.Count)
            {
                ++countItems;
            }

            if (countItems == dataGrid.Items.Count)
            {
                ResizeColumns();
                SetColumnStyle();
                countItems = 0;
            }
        }

        #endregion Дерево и свойства

        #region 3D просмотрщик

        private void DrawingControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.AddedItems)
            {
                //ModelItemIFCObject findObj = tree FindModelObject(item);

                ObservableCollection<BaseModelItemIFC>? collectionObjectModel = treeViewIFC.ItemsSource as ObservableCollection<BaseModelItemIFC>;

                if (collectionObjectModel is null)
                {
                    return;
                }
                ModelItemIFCObject? project = collectionObjectModel[0].ModelItems.OfType<ModelItemIFCObject>().FirstOrDefault(); 
                var findObj = ModelItemIFCObject.SelectionNestedItems(project).FirstOrDefault(it => it.GetIFCObjectDefinition().Equals(item));

                if (findObj != null)
                {
                    findObj.ExpandOver();
                    findObj.IsSelected = true;
                    findObj.IsFocusReference = false;
                }
            }
        }


        #endregion 3D просмотрщик


    }
}