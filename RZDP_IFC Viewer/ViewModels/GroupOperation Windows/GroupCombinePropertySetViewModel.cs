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

        public ObservableCollection<object[]> ControlPropertySets { get; set; }


        public ObservableCollection<ModelItemIFCObject> TargetModelObjects { get; set; }


 

        /// <summary>
        /// Получаем имена таблиц которые выбрал пользователь
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

        #region Добавить наборы

        public ICommand CombinePropertySetCommand { get; }

        private void OnCombinePropertySetCommandExecuted(object o)
        {

        }


        private bool CanCombinePropertySetCommandExecute(object o)
        {
            return TargetModelObjects.Count()>0;
        }

        #endregion Добавить наборы

        #endregion Комманды

        public GroupCombinePropertySetViewModel()
        {}

        public GroupCombinePropertySetViewModel(IEnumerable<ModelItemIFCObject> modelElementsForSearch)
        {
            TargetModelObjects = new(modelElementsForSearch);
            ControlPropertySets = new(modelElementsForSearch.SelectMany(it => it.CollectionPropertySet).
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