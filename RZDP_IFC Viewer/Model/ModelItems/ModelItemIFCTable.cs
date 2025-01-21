using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using IFC_Table_View.HelperIFC;
using IFC_Table_View.IFC.Model;
using IFC_Table_View.View.Windows;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.MeasureResource;

namespace IFC_Table_View.IFC.ModelItem
{
    public class ModelItemIFCTable : BaseModelReferenceIFC
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="IFCTable"></param>
        /// <param name="modelIFC"></param>
        public ModelItemIFCTable(IfcStore ifcStore, IIfcTable IFCTable, ModelIFC modelIFC) : base(ifcStore, IFCTable, modelIFC)
        {
            this.IFCTable = ReplaceSymbols(IFCTable);
            dataTable = FillDataTable(this.IFCTable);
        }

        #region Свойства

        public IIfcTable IFCTable { get; set; }

        public string IFCTableName { 
            get
            {
                return IFCTable.Name;
            }
            set
            {
                Model.ChangeName(new List<(IIfcTable, string)> { (IFCTable, value) });
                OnPropertyChanged("IFCTableName");
            }
        }


        public DataTable dataTable { get; private set; }

        public override string NameReference => IFCTable.Name;

        #endregion Свойства

        #region Методы

        /// <summary>
        /// Заполнение DataTable
        /// </summary>
        /// <param name="ifcTable"></param>
        /// <returns></returns>
        public static DataTable FillDataTable(IIfcTable ifcTable)
        {
            if (ifcTable == null && ifcTable.Rows.Count == 0)
            {
                return null;
            }

            DataTable dataTable = new DataTable();

            dataTable.TableName = ifcTable.Name;
            for (int i = 0; i < ifcTable.Rows[0].RowCells.Count(); i++)
            {
                string nameColumn = ifcTable.Rows[0].RowCells[i].Value.ToString();
                HelreptReplaceSymbols.ReplacingSymbols(ref nameColumn);

                dataTable.Columns.Add(nameColumn);
            }

            for (int i = 1; i < ifcTable.Rows.Count; i++)
            {
                DataRow row = dataTable.NewRow();

                for (int j = 0; j < ifcTable.Rows[i].RowCells.Count(); j++)
                {
                    row[dataTable.Columns[j].ColumnName] = ifcTable.Rows[i].RowCells[j].Value.ToString();
                }
                dataTable.Rows.Add(row);
            }
            return dataTable;
        }

        public IIfcTable GetIFCTable()
        {
            return IFCTable;
        }

        /// <summary>
        /// Замена запрещенных символов
        /// </summary>
        private IIfcTable ReplaceSymbols(IIfcTable ifcTable)
        {
            for (int row = 0; row < ifcTable.Rows.Count; row++)
            {
                IIfcTableRow tt = ifcTable.Rows[row];

                for (int cell = 0; cell < ifcTable.Rows[row].RowCells.Count; cell++)
                {
                    string valueString = ifcTable.Rows[row].RowCells[cell].Value.ToString();

                    string newValueString = Regex.Replace(valueString, @"\s+", " ");

                    newValueString = newValueString.Replace("\0", "");

                    newValueString = newValueString.Trim((char)32);

                    newValueString = newValueString.Replace("измере-ния", "измерения");

                    newValueString = newValueString.Replace("изме- рения", "измерения");

                    newValueString = newValueString.Replace("оли-чество", "оличество");

                    newValueString = newValueString.Replace("ед_,", "ед");

                    newValueString = newValueString.Replace("Ед_", "Ед");

                    ifcTable.Rows[row].RowCells[cell] = new IfcText(newValueString);
                }
            }

            return ifcTable;
        }

        #endregion Методы

        #region Комманды

        #region Удалить_таблицу

        protected override void OnDeleteReferenceCommandExecuted(object o)
        {
            MessageBoxResult result = MessageBox.Show($"Удалить таблицу \"{IFCTableName}?\"", "Внимание!", MessageBoxButton.OKCancel, MessageBoxImage.Question);

            if (result != MessageBoxResult.OK)
            { return; }

            //base.OnDeleteReferenceCommandExecuted(o);

            Model.DeleteIFCObjectReferenceSelect(this);
        }
        #endregion Удалить_таблицу

        #region Открыть_таблицу
        protected override void OnOpenCommandExecuted(object o)
        {
            TableWindow.CreateTableWindow(this);
        }

        #endregion 
        #endregion Комманды
    }
}