using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using IFC_Table_View.HelperExcel;
using IFC_Table_View.IFC.ModelItem;
using Xbim.Ifc4.Interfaces;

namespace IFC_Table_View.View.Windows
{
    /// <summary>
    /// Логика взаимодействия для WindowTable.xaml
    /// </summary>
    public partial class WindowTable : Window, INotifyPropertyChanged
    {
        public DataTable dataTable { get; set; }

        public WindowTable(IIfcTable ifcTable)
        {
            InitializeComponent();

            dataTable = ModelItemIFCTable.FillDataTable(ifcTable);
            DataContext = this;

            MaxWidth = SystemParameters.PrimaryScreenWidth;
            MaxHeight = SystemParameters.PrimaryScreenHeight;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Событие изменения элемента
        /// </summary>
        /// <param name = "PropertyName" ></ param >
        protected virtual void OnPropertyChanged(string PropertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
            }
        }

        private int _FontSizeTable { get; set; } = 12;

        public int FontSizeTable
        {
            get { return _FontSizeTable; }
            set
            {
                if (value < 10)
                {
                    _FontSizeTable = 10;
                }
                else if (value > 30)
                {
                    _FontSizeTable = 30;
                }
                else
                {
                    _FontSizeTable = value;
                }
                ResizeColumns();
                OnPropertyChanged("FontSizeTable");
            }
        }

        private void ResizeColumns()
        {
            foreach (var column in dgTable.Columns)
            {
                column.Width = DataGridLength.SizeToHeader;
                var sizeToHeader = column.Width.DesiredValue;

                column.Width = DataGridLength.SizeToCells;
                var sizeToCells = column.Width.DesiredValue;

                if (sizeToHeader > sizeToCells)
                {
                    column.Width = DataGridLength.SizeToHeader;
                }
                else
                {
                    column.Width = DataGridLength.SizeToCells;
                }
            }
        }

        private void SetColumnStyle()
        {
            foreach (DataGridTextColumn column in dgTable.Columns)
            {
                Style columnStyle = new Style(typeof(TextBlock));
                columnStyle.Setters.Add(new Setter(
                                                    TextBlock.TextWrappingProperty,
                                                    TextWrapping.Wrap
                                                    ));

                column.ElementStyle = columnStyle;
            }
        }

        private void Button_More_Click(object sender, RoutedEventArgs e)
        {
            ++FontSizeTable;
        }

        private void Button_Less_Click(object sender, RoutedEventArgs e)
        {
            --FontSizeTable;
        }

        private void dgTable_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetColumnStyle();
        }

        private void Button_Export_Excel_Click(object sender, RoutedEventArgs e)
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
    }
}