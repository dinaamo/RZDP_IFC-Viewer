using System.CodeDom;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.OleDb;
using System.Windows;
using System.Windows.Input;
using Editor_IFC;
using RZDP_IFC_Viewer.IFC.Model.ModelObjectPropertySet.Base;
using RZDP_IFC_Viewer.IFC.ModelItem;
using RZDP_IFC_Viewer.Infracrucrure.Commands;
using RZDP_IFC_Viewer.ViewModels.Base;
using Xbim.Ifc4.Interfaces;

namespace RZDP_IFC_Viewer.ViewModels
{
    internal class GroupCombinePropertySetViewModel : BaseViewModel
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
        private string _NameNewPropertySet;
        public string NameNewPropertySet
        {
            get
            {
                return _NameNewPropertySet;
            }
            set
            {
                _NameNewPropertySet = value;
                OnPropertyChanged("NameNewPropertySet");
            }
        }

        public ObservableCollection<object[]> ControlPropertySets { get; set; }


        public ObservableCollection<ModelItemIFCObject> TargetModelObjects { get; set; }


        void CombinePropertySets()
        {
            int countAddPropertySet = 0;
            int countMissedPropertySet = 0;

            IEnumerable<string> selectNamePropertySets = GetNamesPropertySets();

            foreach (ModelItemIFCObject modelObject in TargetModelObjects)
            {
                //Получаем наборы в объекте
                IEnumerable<BasePropertySetDefinition> propertySetsModelObject = modelObject.CollectionPropertySet;
                                                            //Where(it => it is EdPropertySet2x3 || it is EdPropertySet4);
                //Выбираем имена наборов
                var namePropertySetsModelObject = propertySetsModelObject.Select(it => it.NamePropertySet);

                //Если в наборах объекта нет наборов выбранных пользователем пропускаем цикл
                if (!namePropertySetsModelObject.Any(it => selectNamePropertySets.Contains(it)))
                {
                    ++countMissedPropertySet;
                    continue;
                }
                //Готовим общую коллекцию параметров
                List<(string, object)> properties = new();

                //Проходим циклом по выбранным наборам
                foreach (string propertySetSelect in selectNamePropertySets)
                {
                    //Проходим по наборам объекта с таким же именем
                    foreach (BasePropertySetDefinition targetPropertySetModelObject in propertySetsModelObject.Where(it => it.NamePropertySet.Equals(propertySetSelect)))
                    {
                        foreach (IPropertyModel<IIfcResourceObjectSelect> propertyModelObject in targetPropertySetModelObject.PropertyCollection)
                        {
                            //Проверяем есть ли в наборе такой же параметр
                            (string, object) findItem = properties.FirstOrDefault(it => it.Item1.Equals(propertyModelObject.NameProperty));
                            if (findItem.Item1 != null)
                            {
                                //Если нашли проверяем равны ли значения
                                if (Convert.ToString(findItem.Item2)  == Convert.ToString(propertyModelObject.Value))
                                {
                                    //Если равны то пропускаем цикл
                                    continue;
                                }
                            }
                            //Добавляем параметр в общую коллекцию параметров
                            properties.Add((propertyModelObject.NameProperty, propertyModelObject.Value));
                        }
                    }
                }
                ++countAddPropertySet;
                //Запускаем добавляем набор
                modelObject.ModelObjectEditor.CreateNewPropertySet(NameNewPropertySet, properties);
                modelObject.OnPropertyChanged("CollectionPropertySet");

            }
            Status = $"Количество добавленных наборов: {countAddPropertySet}\n" +
                        $"Количество пропущенных элементов: {countMissedPropertySet}";
        }
 

        /// <summary>
        /// Получаем имена наборов которые выбрал пользователь
        /// </summary>
        IEnumerable<string> GetNamesPropertySets()
        { 
            foreach (var controlItem in ControlPropertySets)
            {
                object stateCell = controlItem[0];
                if (stateCell != null && (bool)stateCell)
                {
                    yield return controlItem[1].ToString();
                }
            }
        }



        #region Комманды 

        #region Объединить наборы

        public ICommand CombinePropertySetCommand { get; }

        private void OnCombinePropertySetCommandExecuted(object o)
        {
            if (!(GetNamesPropertySets()?.Count() > 0))
            {
                MessageBox.Show("Укажите объединяемые наборы", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (string.IsNullOrEmpty(NameNewPropertySet))
            {
                MessageBox.Show("Задайте имя набора характеристик", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            TargetModelObjects[0].ModelIFC.ActionInTransaction(new List<Action>() { CombinePropertySets });
        }


        private bool CanCombinePropertySetCommandExecute(object o)
        {
            return TargetModelObjects.Count()>0;
        }

        #endregion Объединить наборы 



        #endregion Комманды

        public GroupCombinePropertySetViewModel()
        {}

        public GroupCombinePropertySetViewModel(IEnumerable<ModelItemIFCObject> modelElementsForSearch)
        {
            TargetModelObjects = new(modelElementsForSearch);
            ControlPropertySets = new(modelElementsForSearch.SelectMany(it => it.CollectionPropertySet).
                                                                //Where(it => it is EdPropertySet2x3 || it is EdPropertySet4).
                                                                Select(it => it.NamePropertySet).Distinct().
                                                                Select(it => new object[] {false, it }));

            #region Комманды

            CombinePropertySetCommand = new ActionCommand(
                OnCombinePropertySetCommandExecuted,
                CanCombinePropertySetCommandExecute);





            #endregion Комманды
        }
    }

}