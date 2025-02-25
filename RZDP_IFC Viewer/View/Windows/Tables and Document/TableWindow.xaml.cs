﻿using System.Data;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RZDP_IFC_Viewer.IFC.ModelItem;
using RZDP_IFC_Viewer.ViewModels;

namespace RZDP_IFC_Viewer.View.Windows
{
    /// <summary>
    /// Логика взаимодействия для WindowTable.xaml
    /// </summary>
    public partial class TableWindow : Window
    {
        private TableWindow(ModelItemIFCTable modelItemIFCTable)
        {
            InitializeComponent();

            DataContext = new TableWindowViewModel(modelItemIFCTable);

            MaxWidth = SystemParameters.PrimaryScreenWidth;
            MaxHeight = SystemParameters.PrimaryScreenHeight;
        }

        private static TableWindow instance;

        public async static void CreateTableWindow(ModelItemIFCTable modelItemIFCTable)
        {
            if (instance == null)
            {
                instance = new TableWindow(modelItemIFCTable);
                instance.Show();
                instance.Topmost = true;
                
                await Task.Delay(500);
                instance.Activate();
                instance.Topmost = false;
            }
            else
            {
                instance.WindowState = WindowState.Normal;
                instance.Topmost = true;
                instance.Topmost = false;
                return;
            }
        }


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

        private int countItems = 0;

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ResizeColumns();
        }

        private void Win_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            instance = null;
        }


        private void Button_Click_Search(object sender, RoutedEventArgs e)
        {
            ResetOnlyDataGrid();
            ((TableWindowViewModel)DataContext).SearchCells(dgTable, CHBFullText.IsChecked, CHBIgnorRegister.IsChecked, TBСonditionsSearch.Text);
        }

        private void Button_Click_Reset_Search_Full(object sender, RoutedEventArgs e)
        {
            ResetOnlyDataGrid();
            CHBFullText.IsChecked = false;
            CHBIgnorRegister.IsChecked = false;
            TBСonditionsSearch.Text = string.Empty;
        }


        void ResetOnlyDataGrid()
        {
            ((TableWindowViewModel)DataContext).ResetSearch();
            foreach (DataRowView rowView in dgTable.Items)
            {
                DataGridRow row = dgTable.ItemContainerGenerator.ContainerFromItem(rowView) as DataGridRow;

                foreach (var column in dgTable.Columns)
                {
                    TextBlock cell = column.GetCellContent(row) as TextBlock;

                    if (cell != null)
                        cell.Background = null;
                }
            }
        }
    }
}