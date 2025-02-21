using System.Collections.ObjectModel;
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

        private ObservableCollection<(DataGridCell, TextBlock)> _findedCells;

        private int _IndexCurrentCell;

        public int IndexCurrentCell
        {
            get => _IndexCurrentCell;
            set => Set(ref _IndexCurrentCell, value);
        }

        public int CountFoundElements
        {
            get
            {
                return _findedCells.Count();
            }
        }

        private void FindedCells_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("CountFoundElements");
        }

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


        public void SearchCells(DataGrid dataGrid, bool? isFullText, bool? isIgnorRegister, string seachString)
        {
            if (seachString.Equals(string.Empty))
            { return; }
            
            foreach (DataRowView rowView in dataGrid.Items)
            {
                DataGridRow row = dataGrid.ItemContainerGenerator.ContainerFromItem(rowView) as DataGridRow;

                foreach (var column in dataGrid.Columns)
                {
                    TextBlock textBlock = column.GetCellContent(row) as TextBlock;
                    
                    string cellString = textBlock?.Text ?? "";

                    if (isIgnorRegister == false)
                    {
                        cellString = cellString.ToLower();
                        seachString = seachString.ToLower();
                    }

                    if (isFullText == true)
                    {
                        if (cellString.Equals(seachString))
                        {
                            textBlock.Background = Brushes.Tomato;
                            _findedCells.Add(((DataGridCell)textBlock.Parent, textBlock));  
                        }
                    }
                    else
                    {
                        if (cellString.Contains(seachString))
                        {
                            textBlock.Background = Brushes.Tomato;
                            _findedCells.Add(((DataGridCell)textBlock.Parent, textBlock));
                        }
                    }
                }
            }

        }



        #endregion Поиск

        #region Сброс


        public void ResetSearch()
        {
            _findedCells.Clear();
            IndexCurrentCell = 0;
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

        #region Шаг по найденным ячейкам

        public ICommand MoveNextToCellCommand { get; }

        private void OnMoveNextToCellCommandExecuted(object o)
        {
            _findedCells.ToList().ForEach(it => { 
                it.Item2.Background = Brushes.Tomato;
            });

            if (o is bool isNext)
            {
                if (isNext)
                {
                    ++IndexCurrentCell;
                }
                else
                {
                    --IndexCurrentCell;
                }
            }
            DataGridCell cell;
            if (IndexCurrentCell == 0)
            {
                _findedCells[IndexCurrentCell].Item2.Background = Brushes.BlueViolet;
                cell = _findedCells[IndexCurrentCell].Item1;
                IndexCurrentCell = 1;
            }
            else
            {
                _findedCells[IndexCurrentCell - 1].Item2.Background = Brushes.BlueViolet;
                cell = _findedCells[IndexCurrentCell - 1].Item1;
            }

            
            cell.IsSelected = true;
            cell.Focus();



        }

        private bool CanMoveNextToCellCommandExecute(object o)
        {
            if (o is bool isNext)
            {
                if(isNext)
                {
                    return _findedCells.Count() > 0 && IndexCurrentCell <= _findedCells.Count()-1;
                }
                else
                {
                    return _findedCells.Count() > 0 && IndexCurrentCell > 1;
                }
            }
            return false;

        }

        #endregion Шаг по найденным ячейкам

        #endregion Комманды

        public TableWindowViewModel()
        {
        }

        public TableWindowViewModel(ModelItemIFCTable modelTable)
        {

            _findedCells = new();
            _findedCells.CollectionChanged += FindedCells_CollectionChanged;
            this.ModelTable = modelTable;

            Title = this.dataTable.TableName;

            #region Комманды

            MoreSizeFontCommand = new ActionCommand(
                OnMoreSizeFontCommandExecuted,
                CanMoreSizeFontCommandExecute);

            LessSizeFontCommand = new ActionCommand(
                OnLessSizeFontCommandExecuted,
                CanLessSizeFontCommandExecute);

            //SearchCellsCommand = new ActionCommand(
            //    OnSearchCellsCommandExecuted,
            //    CanSearchCellsCommandExecute);

            //ResetSearchCommand = new ActionCommand(
            //    OnResetSearchCommandExecuted,
            //    CanResetSearchCommandExecute);

            ExportToExcelCommand = new ActionCommand(
                OnExportToExcelCommandExecuted,
                CanExportToExcelCommandExecute);

            MoveNextToCellCommand = new ActionCommand(
                OnMoveNextToCellCommandExecuted,
                CanMoveNextToCellCommandExecute);

            //MoveBackToCellCommand = new ActionCommand(
            //    OnMoveBackToCellCommandExecuted,
            //    CanMoveBackToCellCommandExecute);

            #endregion Комманды
        }


    }
}