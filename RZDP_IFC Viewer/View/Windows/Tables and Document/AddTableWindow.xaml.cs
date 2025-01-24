using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RZDP_IFC_Viewer.ViewModels;

namespace IFC_Viewer.View.Windows
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class AddTableWindow : Window
    {
        public AddTableWindow(Action<DataTable> AddTableToTheFile)
        {
            InitializeComponent();

            DataContext = new AddTableWindowViewModel(AddTableToTheFile);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V)
            {
                if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                {
                    dataGridTable.ItemsSource = null;
                    dataGridTable.ItemsSource = AddTableWindowViewModel.CreateDataTableFromClipboard().AsDataView();
                }
            }
        }

        private int countItems = 0;

        private void dgTable_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (countItems < dataGridTable.Items.Count)
            {
                ++countItems;
            }
            if (countItems == dataGridTable.Items.Count)
            {
                ResizeColumns();
                SetColumnStyle();
                countItems = 0;
            }
        }

        private void ResizeColumns()
        {
            foreach (var column in dataGridTable.Columns)
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
                    column.Width = new DataGridLength(dataGridTable.Width / dataGridTable.Columns.Count, DataGridLengthUnitType.Auto);
                }
            }
        }

        private void SetColumnStyle()
        {
            Style columnStyle = new Style(typeof(TextBlock));
            columnStyle.Setters.Add(new Setter(
                                                TextBlock.TextWrappingProperty,
                                                TextWrapping.Wrap
                                                ));
            foreach (DataGridTextColumn column in dataGridTable.Columns)
            {
                column.ElementStyle = columnStyle;
            }
        }
    }
}