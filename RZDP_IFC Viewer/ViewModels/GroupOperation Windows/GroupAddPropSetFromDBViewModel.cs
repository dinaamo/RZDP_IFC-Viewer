using System.Collections.ObjectModel;
using System.Data;
using System.Data.OleDb;
using System.Windows;
using System.Windows.Input;
using RZDP_IFC_Viewer.IFC.ModelItem;
using RZDP_IFC_Viewer.Infracrucrure.Commands;
using RZDP_IFC_Viewer.ViewModels.Base;

namespace RZDP_IFC_Viewer.ViewModels
{
    internal class GroupAddPropSetFromDBViewModel : BaseViewModel
    {
        private string _Status;

        public string Status
        {
            get
            {
                return _Status;
            }
            set
            {
                _Status = value;
                OnPropertyChanged("Status");
            }
        }

        private string _DBPath;

        public string DBPath 
        {
            get 
            { 
                return _DBPath; 
            }
            set 
            {
                _DBPath = value;
                OnPropertyChanged("DBPath");
            }
        }

        public ObservableCollection<object[]> ControlTables { get; set; }


        private string _connectionString
        {
            get { return $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={DBPath}"; }
        }

        List<ElementTable> _elementTableCollection;


        public ObservableCollection<ModelItemIFCObject> TargetModelObjects { get; set; }


        CSharp.OleDb.OleDbHelper _csOledb;

        System.Data.OleDb.OleDbConnection _connection;

        private bool ConnectDataBase()
        {
            string? path = HelperFileIFC.OpenFileAccess();

            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            DBPath = path;

            try
            {
                _csOledb = new CSharp.OleDb.OleDbHelper();

                _connection = _csOledb.CreateConnection(_connectionString);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Невозможно открыть базу данных\n" +
                    $"{ex.Message}", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        private DataTable FillDataGridTable()
        {
            _connection.Open();
            DataTable datatable = _connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
            return datatable;
        }

        /// <summary>
        /// Получаем имена таблиц которые выбрал пользователь
        /// </summary>
        Dictionary<string, string> GetNameTable()
        {
            Dictionary<string, string> tableNames = new Dictionary<string, string>();

            foreach (var controlItem in ControlTables)
            {
                object stateCell = controlItem[2];
                if (stateCell != null && (bool)stateCell)
                {
                    tableNames.Add(controlItem[0].ToString(), controlItem[1].ToString());
                }
            }
            return tableNames;
        }


        /// <summary>
        /// Заполняем коллекцию контрольными экземплярами
        /// </summary>
        List<ElementTable> FillElementTableCollection()
        {
            List<ElementTable> elementTableCollection = new();
 
            foreach (KeyValuePair<string, string> tableName in GetNameTable())// Проходим по всем таблицам которые выбрал пользователь
            {
                string query = $"SELECT * FROM [{tableName.Key}]";

                OleDbDataAdapter adapter = new OleDbDataAdapter(query, _connection);

                DataSet ds = new DataSet();
                adapter.Fill(ds);

                foreach (DataTable dt in ds.Tables) //Получаем таблицы
                {
                    

                    for (int rowIndex = 0; rowIndex < dt.Rows.Count; rowIndex++) // Проходим по всем строкам таблицы
                    {
                        DataRow row = dt.Rows[rowIndex]; //Получаем текущую строку

                        // Заполняем коллекцию параметров
                        List<(string, object)> collectionParameters = new();
                        for (int cellIndex = 4; cellIndex < row.ItemArray.Count(); cellIndex++) // Проходим по всем ячейкам строки
                        {
                            collectionParameters.Add(
                                    (dt.Columns[cellIndex].ColumnName, row.ItemArray[cellIndex].ToString())); //Кортеж наименование параметра-значение 
                        }
                        elementTableCollection.Add(new ElementTable( //Добавляем в коллекцию контрольный экземпляр
                                                                    row.ItemArray[0].ToString(), //Контрольный фрагмент имени
                                                                    row.ItemArray[1].ToString(), //Контрольное имя типа IFC 
                                                                    row.ItemArray[2].ToString(), //Контрольное значение типа объекта
                                                                    row.ItemArray[3].ToString(), //Контрольное значение описание объекта
                                                                    tableName.Value, //Название набора берем из 2-й ячейки строки
                                                                    collectionParameters)); //Набор характеристик
                    }
                }  
            }
            return elementTableCollection;
        }


        /// <summary>
        /// Создаем в элементах новые наборы
        /// </summary>
        void FillElementsToPropertySet()
        {
            int countAddPropertySet = 0;
            int countAddProperty = 0;
            int countRecordProperty = 0;
            int countMissing = 0;

            foreach (ElementTable controlElement in _elementTableCollection)
            {
                //Фильтруем элементы по критериям из таблицы
                string name = controlElement.NameFragment.ToLower();

                //Фильтруем по классу
                IEnumerable<ModelItemIFCObject> filterCollect1 = TargetModelObjects. 
                                            Where(it => it.IFCClass.ToLower() == controlElement.IFCClass.ToLower());

                //Фильтруем по имени
                IEnumerable<ModelItemIFCObject> filterCollect2 = filterCollect1.Where(it => it.IFCObjectName.Contains(controlElement.NameFragment, StringComparison.OrdinalIgnoreCase));

                //Фильтруем по типу объекта
                IEnumerable<ModelItemIFCObject> filterCollect3 = filterCollect2.Where(it => it.IFCObjectType.Contains(controlElement.IFCType, StringComparison.OrdinalIgnoreCase));

                //Фильтруем по описанию объекта
                IEnumerable<ModelItemIFCObject> filterCollect4 = filterCollect3.Where(it => it.IFCDescription.Contains(controlElement.Description, StringComparison.OrdinalIgnoreCase));


                //Проходим по всем найденным элементам
                foreach (ModelItemIFCObject modelItem in filterCollect4)
                {
                    //Пытаемся найти набор по названию
                    var propertySetDef = modelItem.CollectionPropertySet.FirstOrDefault(it => it.NamePropertySet == controlElement.NamePropertySet);

                    //Если находим
                    if (propertySetDef is not null)
                    {
                        //Проходим по всем проверяемым параметрам
                        foreach (var property in controlElement.CollectionParameters)
                        {
                            //Пробуем найти параметр с таким же названием
                            var targetProp = propertySetDef.PropertyCollection.FirstOrDefault(it => it.NameProperty == property.Item1);

                            if (targetProp is not null)//Если находим то задаем значение
                            {
                                targetProp.SetNewValue(property.Item2.ToString());
                                countRecordProperty++;
                            }
                            else //Если нет то добавляем новый параметр
                            {
                                propertySetDef.AddProperty(property.Item1, property.Item2.ToString());
                                countAddProperty++;
                            }
                        }
                    }
                    else //Если такого набора нет до добавляем новый набор с параметрами
                    {
                        modelItem.ModelObjectEditor.CreateNewPropertySet(controlElement.NamePropertySet, controlElement.CollectionParameters);  
                        ++countAddPropertySet;
                    }
                }
            }
            Status = $"Количество добавленных наборов: {countAddPropertySet}\n" +
                                $"Количество добавленных параметров: {countAddProperty}\n" +
                                $"Количество измененных параметров: {countRecordProperty}\n" +
                                $"Количество пропущенных элементов: {countMissing}";
        }


        #region Комманды 

        #region Выбрать БД

        public ICommand SelectDBCommand { get; }

        private void OnSelectDBCommandExecuted(object o)
        {

            if (!ConnectDataBase())
            {
                return;
            }

            ControlTables.Clear();
            DataTable dataTable = FillDataGridTable();

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                ControlTables.Add(new object[] { dataTable.Rows[i][2], dataTable.Rows[i][2], false });
            }
        }


        private bool CanSelectDBCommandExecute(object o)
        {
            return true;
        }

        #endregion Выбрать БД


        #region Добавить наборы

        public ICommand AddPropertySetCommand { get; }

        private void OnAddPropertySetCommandExecuted(object o)
        {
            _elementTableCollection = FillElementTableCollection();
            TargetModelObjects[0].ModelIFC.ActionInTransaction(new List<Action>() { FillElementsToPropertySet });
        }


        private bool CanAddPropertySetCommandExecute(object o)
        {
            return TargetModelObjects.Count()>0;
        }

        #endregion Добавить наборы

        #endregion Комманды

        public GroupAddPropSetFromDBViewModel()
        {}

        public GroupAddPropSetFromDBViewModel(IEnumerable<ModelItemIFCObject> modelElementsForSearch)
        {
            TargetModelObjects = new(modelElementsForSearch);
            ControlTables = new();
            #region Комманды

            SelectDBCommand = new ActionCommand(
                OnSelectDBCommandExecuted,
                CanSelectDBCommandExecute);

            AddPropertySetCommand = new ActionCommand(
                OnAddPropertySetCommandExecuted,
                CanAddPropertySetCommandExecute);

            #endregion Комманды
        }
    }

    struct ElementTable
    {
        public ElementTable(string name, string type, string objectType, string description, string namePropertySet, List<(string, object)> collectionParameters)
        {
            NameFragment = name;
            IFCClass = type;
            IFCType = objectType;
            Description = description;
            NamePropertySet = namePropertySet;
            CollectionParameters = collectionParameters;
        }
        public string NameFragment { get; }
        public string IFCClass { get; }
        public string IFCType { get; }
        public string Description { get; }
        public string NamePropertySet {  get; }
        public List<(string, object)> CollectionParameters { get; }
    }
}