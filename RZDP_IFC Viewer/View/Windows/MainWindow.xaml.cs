using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using IFC_Table_View.IFC.ModelItem;
using IFC_Table_View.Infracrucrure.FindObjectException;
using IFC_Table_View.View.Windows;
using IFC_Table_View.ViewModels;
using RZDP_IFC_Viewer.IFC.Model.ModelObjectPropertySet.Base;
using Xbim.Ifc4.Interfaces;

namespace IFC_Table_View
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //IProgress<(double percente, string message)> progress;

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

                            BaseModelReferenceIFC targetBaseReferenceModel = collectionObjectModel[0].ModelItems.
                                                                    OfType<BaseModelReferenceIFC>().
                                                                    FirstOrDefault(it => it.GetReference().Equals(ifcPropertyRefVa?.PropertyReference));

                            if (targetBaseReferenceModel is ModelItemIFCTable modelIfcTable)
                            {
                                new TableWindow(modelIfcTable).ShowDialog();
                            }
                            else if (targetBaseReferenceModel is ModelItemDocumentReference modelItemDocumentReference)
                            {
                                modelItemDocumentReference.OpenDocument();
                            }
                        }
                    }
                    else if (textBlock.DataContext is ModelItemIFCObject searchModelObject)
                    {
                        ModelItemIFCObject findObj = FindModelObject(searchModelObject.ItemIFC);
                        if (findObj != null)
                        {
                            findObj.IsSelected = true;
                            findObj.IsFocusReference = false;
                        }
                    }
                }
            }
        }

        private ModelItemIFCObject FindModelObject(object searchModelObject)
        {
            if (searchModelObject == null)
            {
                return null;
            }
            try
            {
                ((BaseModelItemIFC)treeViewIFC.Items[0]).IsExpanded = true;

                IEnumerable<ModelItemIFCObject> secondLevelCollection = ((BaseModelItemIFC)treeViewIFC.Items[0]).ModelItems.
                    OfType<ModelItemIFCObject>();

                foreach (ModelItemIFCObject modelObject in secondLevelCollection)
                {
                    if (modelObject.ItemIFC.Equals(searchModelObject))
                    {
                        throw new FindObjectException(modelObject);
                    }
                    ModelItemIFCObject.FindSingleTreeObject(modelObject, searchModelObject);
                }
            }
            catch (FindObjectException findObj)
            {
                return findObj.FindObject;
            }

            return null;
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
                                                                        FirstOrDefault(it => it.ItemIFC == PropertyRefVa?.PropertyReference);

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
                                                                        FirstOrDefault(it => it.ItemIFC == PropertyRefVa?.PropertyReference);

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
                //DateTime dt1 = DateTime.Now;

                ModelItemIFCObject findObj = FindModelObject(item);
                if (findObj != null)
                {
                    //new Task(() =>
                    //{
                    //MainWindowIFC.Dispatcher.InvokeAsync(() =>
                    //{
                    findObj.ExpandOver();
                    findObj.IsSelected = true;
                    findObj.IsFocusReference = false;
                    //});
                    //}).Start();

                    //DateTime dt2 = DateTime.Now;
                    //TimeSpan tsp1 = dt2 - dt1;

                    //MessageBox.Show(tsp1.TotalMilliseconds.ToString());
                }
            }
        }

        #endregion 3D просмотрщик
    }
}