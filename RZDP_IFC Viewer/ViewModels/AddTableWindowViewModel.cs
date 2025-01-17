using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using IFC_Table_View.Infracrucrure.Commands;
using IFC_Table_View.ViewModels.Base;

namespace IFC_Table_View.ViewModels
{
    internal class AddTableWindowViewModel : BaseViewModel
    {
        private Action<DataTable> AddTableToTheFile;

        //DataTable _dataTable;

        //public DataTable dataTable
        //{
        //    get => _dataTable;
        //    set => Set(ref _dataTable, value);
        //}

        #region Вставить из буфера обмена

        public ICommand PasteFromClipboardCommand { get; }

        private void OnPasteFromClipboardCommandExecuted(object o)
        {
            try
            {
                object[] ControlArray = (object[])o;
                DataGrid dataGrid = ControlArray[0] as DataGrid;
                TextBox textBoxTable = ControlArray[1] as TextBox;

                textBoxTable.Text = string.Empty;

                dataGrid.ItemsSource = null;

                dataGrid.ItemsSource = CreateDataTableFromClipboard().AsDataView();
            }
            catch (FormatException fEx)
            {
                MessageBox.Show(fEx.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool CanPasteFromClipboardCommandExecute(object o)
        {
            return true;
        }

        #endregion Вставить из буфера обмена

        #region Очистить

        public ICommand ClearCommand { get; }

        private void OnClearCommandExecuted(object o)
        {
            object[] ControlArray = (object[])o;
            DataGrid dataGrid = ControlArray[0] as DataGrid;
            TextBox textBoxTable = ControlArray[1] as TextBox;

            textBoxTable.Text = string.Empty;

            dataGrid.ItemsSource = null;
        }

        private bool CanClearCommandExecute(object o)
        {
            return true;
        }

        #endregion Очистить

        #region Добавить в файл таблицу

        public ICommand AddTableCommand { get; }

        private void OnAddTableCommandExecuted(object o)
        {
            object[] ControlArray = (object[])o;
            DataGrid dataGrid = ControlArray[0] as DataGrid;
            TextBox textBoxTable = ControlArray[1] as TextBox;

            DataView dataView = dataGrid.ItemsSource as DataView;

            DataTable dataTable = dataView?.Table;

            if (textBoxTable.Text == string.Empty)
            {
                MessageBox.Show("Задайте имя таблицы", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dataTable is not null)
            {
                dataTable.TableName = textBoxTable.Text;
                AddTableToTheFile(dataTable);
                textBoxTable.Text = string.Empty;
                dataGrid.ItemsSource = null;
            }
        }

        private bool CanAddTableCommandExecute(object o)
        {
            return true;
        }

        #endregion Добавить в файл таблицу

        public static DataTable CreateDataTableFromClipboard()
        {
            if (!Clipboard.ContainsText())
            {
                throw new FormatException("Неверный формат данных");
            }

            DataTable dataTable = new DataTable();
            //В качестве разделителя по умолчанию используется табуляция, но если табуляция отсутствует, используется системное значение по умолчанию

            //char textSeparator = '\r';

            List<String> clipboardAsList = new List<String>(Clipboard.GetText().Split('\r'));

            //List<String[]> cleanLines = clipboardAsList
            // .Select(s => s.Replace("\n", "").Replace("\r", "").Split(textSeparator.ToCharArray()))
            // .ToList<String[]>();

            List<String[]> cleanLines = clipboardAsList
                 .Select(s => s.Trim('\n', '\r').Replace('\"', '\0').Split('\t'))
                 .ToList<String[]>();

            String[] line = cleanLines[0];
            for (int i = 0; i < line.Length; i++)
            {
                string columnName = line[i];
                columnName = ReplaceSymbols(columnName);
                columnName = columnName.Replace("/", "");
                columnName = columnName.Replace(".", "");
                dataTable.Columns.Add(columnName);
            }

            for (int ln = 1; ln < cleanLines.Count - 1; ln++)
            {
                line = cleanLines[ln];
                DataRow dataRow = dataTable.NewRow();

                //if (line.Length > dataRow.ItemArray.Length)
                //{
                //    throw new FormatException("Буфер обмена содержит следующее " + line.Length + " колонки: \n\n" + string.Join(", " + Environment.NewLine, line) + ".\n\nНо в Datagrid содержится только" + dataRow.ItemArray.Length + " колонки.");
                //}

                for (int i = 0; i < line.Length; i++)
                {
                    dataRow[i] = ReplaceSymbols(line[i]);
                }

                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        private static string ReplaceSymbols(string targetString)
        {
            string newValueString = Regex.Replace(targetString, @"\s+", " ");

            newValueString = newValueString.Replace("\0", "");

            newValueString = Regex.Replace(newValueString, @"\s+$", "");

            char ch = (char)32;

            //newValueString = newValueString.Trim((char)32);

            newValueString = newValueString.Replace("измере-ния", "измерения");

            newValueString = newValueString.Replace("изме- рения", "измерения");

            newValueString = newValueString.Replace("оли-чество", "оличество");

            newValueString = newValueString.Replace("ед_,", "ед");

            newValueString = newValueString.Replace("Ед_", "Ед");

            return newValueString;
        }

        public AddTableWindowViewModel()
        {
        }

        public AddTableWindowViewModel(Action<DataTable> AddTableToTheFile)
        {
            this.AddTableToTheFile = AddTableToTheFile;

            PasteFromClipboardCommand = new ActionCommand(
                OnPasteFromClipboardCommandExecuted,
                CanPasteFromClipboardCommandExecute);

            ClearCommand = new ActionCommand(
                OnClearCommandExecuted,
                CanClearCommandExecute);

            AddTableCommand = new ActionCommand(
                OnAddTableCommandExecuted,
                CanAddTableCommandExecute);
        }
    }
}