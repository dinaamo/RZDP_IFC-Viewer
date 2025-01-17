using System.Data;
using Excel = Microsoft.Office.Interop.Excel;

namespace IFC_Table_View.HelperExcel
{
    internal class ExcelHelper : IDisposable
    {
        private Excel.Application appExcel;
        private Excel.Workbook workbook;
        private Excel.Worksheet worksheet;

        public void WriteData(DataTable dataTable)
        {
            if (dataTable is null)
            { return; }

            appExcel = new Excel.Application();
            appExcel.Visible = false;

            workbook = appExcel.Workbooks.Add();

            FillTable(dataTable);

            //Содержимое по контенту
            Excel.Range range = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[dataTable.Rows.Count + 1, dataTable.Columns.Count + 1]];
            range.EntireColumn.AutoFit();
            range.EntireRow.AutoFit();

            appExcel.Visible = true;
        }

        private void FillTable(DataTable dataTable)
        {
            worksheet = workbook.ActiveSheet as Excel.Worksheet;

            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                string columnName = ReplacingCharacters(dataTable.Columns[i].ColumnName);
                worksheet.Cells[1, i + 1] = columnName;
            }

            for (int row = 0; row < dataTable.Rows.Count; row++)
            {
                for (int col = 0; col < dataTable.Columns.Count; col++)
                {
                    string cellValue = ReplacingCharacters(dataTable.Rows[row][col].ToString());
                    worksheet.Cells[row + 2, col + 1] = cellValue;
                }
            }
        }

        public void Dispose()
        {
            appExcel = null;
            workbook = null;
            worksheet = null;
            GC.Collect();
        }

        private string ReplacingCharacters(string targetString)
        {
            targetString = targetString.Replace("\0", "");

            return targetString;
        }
    }
}