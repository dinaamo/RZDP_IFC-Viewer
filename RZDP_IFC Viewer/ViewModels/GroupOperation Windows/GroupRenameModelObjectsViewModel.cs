using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Editor_IFC;
using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json.Linq;
using RZDP_IFC_Viewer.IFC.ModelItem;
using RZDP_IFC_Viewer.Infracrucrure;
using RZDP_IFC_Viewer.Infracrucrure.Commands;
using RZDP_IFC_Viewer.ViewModels.Base;
using Xbim.Ifc4.Interfaces;

namespace RZDP_IFC_Viewer.ViewModels
{
    internal class GroupRenameModelObjectsViewModel : BaseViewModel
    {
        private ObservableCollection<ModelItemIFCObject> TargetModelObjects;

        private ObservableCollection<ModelItemIFCObject> _FilteredSearchItems;

        public ObservableCollection<ModelItemIFCObject> FilteredSearchItems
        {
            get { return _FilteredSearchItems; }
            set { _FilteredSearchItems = value; }
        }


        public void Search(string fragmentNameModelObject)
        {
            FilteredSearchItems = new(TargetModelObjects.
                                    Where(it => it.IFCObjectName.
                                    Contains(fragmentNameModelObject, StringComparison.OrdinalIgnoreCase)));
        }

        public void ResetSearchConditions()
        {
            FilteredSearchItems = TargetModelObjects;
        }


        #region Комманды 

        #region Переименовать элементы

        public ICommand ReplaceNameModelObjectCommand { get; }

        private void OnReplaceNameModelObjectCommandExecuted(object o)
        {
            object[]? arrParameters =  o as object[];
            string? setValueString = Convert.ToString(arrParameters[0]);
            string? searchingString = Convert.ToString(arrParameters[1]);

            bool setFragment = (bool)arrParameters[2];
            bool setWhole = (bool)arrParameters[3];
            bool setPrefix = (bool)arrParameters[4];

            if (arrParameters[5] is IEnumerable targetPropertySetsSelect)
            {
            }

        }


        private bool CanReplaceNameModelObjectCommandExecute(object o)
        {
            return true;
        }

        #endregion Переименовать элементы

        #region Задать имя из параметра

        public ICommand SetNameFromParameterCommand { get; }

        private void OnSetNameFromParameterCommandExecuted(object o)
        {
            if (o is IEnumerable targetPropertySetsSelect)
            {
            }

        }

        private bool CanSetNameFromParameterCommandExecute(object o)
        {
            return true;
        }

        #endregion Задать имя из параметра



        #endregion Комманды

        public GroupRenameModelObjectsViewModel()
        {}

        public GroupRenameModelObjectsViewModel(IEnumerable<ModelItemIFCObject> modelElementsForSearch)
        {
            TargetModelObjects = new(modelElementsForSearch.Distinct());

            ResetSearchConditions();

            #region Комманды

            ReplaceNameModelObjectCommand = new ActionCommand(
                OnReplaceNameModelObjectCommandExecuted,
                CanReplaceNameModelObjectCommandExecute);

            SetNameFromParameterCommand = new ActionCommand(
                OnSetNameFromParameterCommandExecuted,
                CanSetNameFromParameterCommandExecute);



            #endregion Комманды
        }
    }


}