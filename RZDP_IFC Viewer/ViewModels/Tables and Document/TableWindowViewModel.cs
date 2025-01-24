using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using RZDP_IFC_Viewer.HelperExcel;
using RZDP_IFC_Viewer.IFC.ModelItem;
using RZDP_IFC_Viewer.Infracrucrure.Commands;
using RZDP_IFC_Viewer.ViewModels.Base;

namespace RZDP_IFC_Viewer.ViewModels
{
    internal class TableWindowViewModel : BaseViewModel
    {
        public DataTable dataTable => ModelTable.dataTable;

        private ModelItemIFCTable ModelTable { get; set; }

        public string NameTable
        {
            get
            {
                return ModelTable.IFCTableName;
            }
            set
            {
                ModelTable.IFCTableName = value;
                OnPropertyChanged("IFCTableName");
            }
        }
        
        public string[] СonditionsSearch { get; private set; } = { "Равно", "Не равно", "Содержит", "Не содержит" };

        #region Заголовок

        private string _Title;

        ///<summary>Заголовок окна</summary>
        public string Title
        {
            get => _Title;
            set => Set(ref _Title, value);
        }

        #endregion Заголовок

        private int _FontSizeTable = 12;

        public int FontSizeTable
        {
            get => _FontSizeTable;
            set
            {
                if (value < 10)
                {
                    Set(ref _FontSizeTable, 10);
                }
                else if (value > 30)
                {
                    Set(ref _FontSizeTable, 30);
                }
                else
                {
                    Set(ref _FontSizeTable, value);
                }
            }
        }

        #region Комманды

        #region Шрифт больше

        public ICommand MoreSizeFontCommand { get; }

        private void OnMoreSizeFontCommandExecuted(object o)
        {
            ++FontSizeTable;
        }

        private bool CanMoreSizeFontCommandExecute(object o)
        {
            if (FontSizeTable >= 30)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion Шрифт больше

        #region Шрифт меньше

        public ICommand LessSizeFontCommand { get; }

        private void OnLessSizeFontCommandExecuted(object o)
        {
            --FontSizeTable;
        }

        private bool CanLessSizeFontCommandExecute(object o)
        {
            if (FontSizeTable <= 10)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion Шрифт меньше

        #region Поиск

        public ICommand SearchCellsCommand { get; }

        private void OnSearchCellsCommandExecuted(object o)
        {
            object[] ControlArray = (object[])o;

            DataGrid dataGrid = ControlArray[0] as DataGrid;
            bool isFullText = (bool)ControlArray[1];
            bool isIgnorRegister = (bool)ControlArray[2];
            string seachString = (string)ControlArray[3];

            if (seachString.Equals(string.Empty))
            { return; }

            int CountFound = 0;

            foreach (DataRowView rowView in dataGrid.Items)
            {
                DataGridRow row = dataGrid.ItemContainerGenerator.ContainerFromItem(rowView) as DataGridRow;

                foreach (var column in dataGrid.Columns)
                {
                    TextBlock cell = column.GetCellContent(row) as TextBlock;
                    string cellstring = cell.Text;

                    if (!isIgnorRegister)
                    {
                        cellstring = cellstring.ToLower();
                        seachString = seachString.ToLower();
                    }

                    if (isFullText)
                    {
                        if (cellstring.Equals(seachString))
                        {
                            cell.Background = Brushes.Tomato;
                            ++CountFound;
                        }
                    }
                    else
                    {
                        if (cellstring.Contains(seachString))
                        {
                            cell.Background = Brushes.Tomato;
                            ++CountFound;
                        }
                    }
                }
            }
            ((TextBlock)ControlArray[4]).Text = $"Найдено ячеек: {CountFound}";
        }

        private bool CanSearchCellsCommandExecute(object o)
        {
            return true;
        }

        #endregion Поиск

        #region Сброс

        public ICommand ResetSearchCommand { get; }

        private void OnResetSearchCommandExecuted(object o)
        {
            object[] ControlArray = (object[])o;

            DataGrid dataGrid = ControlArray[0] as DataGrid;
            ((CheckBox)ControlArray[1]).IsChecked = false;
            ((CheckBox)ControlArray[2]).IsChecked = false;
            ((TextBox)ControlArray[3]).Text = string.Empty;

            foreach (DataRowView rowView in dataGrid.Items)
            {
                DataGridRow row = dataGrid.ItemContainerGenerator.ContainerFromItem(rowView) as DataGridRow;

                foreach (var column in dataGrid.Columns)
                {
                    TextBlock cell = column.GetCellContent(row) as TextBlock;

                    cell.Background = null;
                }
            }

            ((TextBlock)ControlArray[4]).Text = string.Empty;
        }

        private bool CanResetSearchCommandExecute(object o)
        {
            return true;
        }

        #endregion Сброс

        #region Экспорт в Excel

        public ICommand ExportToExcelCommand { get; }

        private void OnExportToExcelCommandExecuted(object o)
        {
            using (ExcelHelper excel = new ExcelHelper())
            {
                try
                {
                    excel.WriteData(dataTable);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private bool CanExportToExcelCommandExecute(object o)
        {
            return true;
        }

        #endregion Экспорт в Excel

        #endregion Комманды

        public TableWindowViewModel()
        {
        }

        public TableWindowViewModel(ModelItemIFCTable modelTable)
        {


            this.ModelTable = modelTable;

            Title = this.dataTable.TableName;

            #region Комманды

            MoreSizeFontCommand = new ActionCommand(
                OnMoreSizeFontCommandExecuted,
                CanMoreSizeFontCommandExecute);

            LessSizeFontCommand = new ActionCommand(
                OnLessSizeFontCommandExecuted,
                CanLessSizeFontCommandExecute);

            SearchCellsCommand = new ActionCommand(
                OnSearchCellsCommandExecuted,
                CanSearchCellsCommandExecute);

            ResetSearchCommand = new ActionCommand(
                OnResetSearchCommandExecuted,
                CanResetSearchCommandExecute);

            ExportToExcelCommand = new ActionCommand(
                OnExportToExcelCommandExecuted,
                CanExportToExcelCommandExecute);

            #endregion Комманды
        }
    }
}