using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Editor_IFC;
using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json.Linq;
using RZDP_IFC_Viewer.IFC.ModelItem;
using RZDP_IFC_Viewer.Infracrucrure.Commands;
using RZDP_IFC_Viewer.ViewModels.Base;
using Xbim.Ifc4.Interfaces;

namespace RZDP_IFC_Viewer.ViewModels
{
    internal class GroupEditPropertySetViewModel : BaseViewModel
    {
        private ObservableCollection<ModelItemIFCObject> _targetObjects;

        private ObservableCollection<BasePropertySetDefinition> _FilteredSearchItems;

        public ObservableCollection<BasePropertySetDefinition> FilteredSearchItems
        {
            get
            {
                return _FilteredSearchItems;
            }

            private set
            {
                _FilteredSearchItems = value;
                OnPropertyChanged("FilteredSearchItems");
            }
        }

        #region Очистить фильтр

        public void ResetSearchConditions()
        {
            FilteredSearchItems = new ObservableCollection<BasePropertySetDefinition>(_targetObjects.SelectMany(it => it.CollectionPropertySet));
        }

        #endregion Очистить фильтр

        public void Search(string namePropertySet)
        {
            FilteredSearchItems = new(FilteredSearchItems.Where(it => it.NamePropertySet.
                                    Contains(namePropertySet, StringComparison.OrdinalIgnoreCase)));
        }

        IEnumerable<(IIfcRoot, string)> GetCollectionForChangeName(IEnumerable targetPropertySets, string targetString, string filterString, bool setFragment, bool setWhole, bool setPrefix)
        {
            foreach (BasePropertySetDefinition propertySet in targetPropertySets)
            {
                string resultName = propertySet.NamePropertySet;

                if (setFragment)
                {
                    if (string.IsNullOrEmpty(filterString))
                        continue;
                    resultName = propertySet.NamePropertySet.Replace(filterString, targetString, StringComparison.OrdinalIgnoreCase);
                }
                else if (setWhole)
                {
                    resultName = targetString;
                }
                else if (setPrefix)
                {
                    resultName = targetString + propertySet.NamePropertySet;
                }
                yield return (propertySet.IFCPropertySetDefinition, resultName);
            }
        }



        IEnumerable<(Action<IIfcPropertySetDefinition>, IIfcPropertySetDefinition)> GetPropertySetsToDelete(IEnumerable targetPropertySetsSelect)
        {
            List<(Action<IIfcPropertySetDefinition>, IIfcPropertySetDefinition)> list = new();
            foreach (BasePropertySetDefinition propertySet in targetPropertySetsSelect)
            {
                if (!list.Any(it => it.Item2 == propertySet.IFCPropertySetDefinition))
                {
                    yield return (propertySet.ModelObject.DeletePropertySet, propertySet.IFCPropertySetDefinition);
                }
                list.Add((propertySet.ModelObject.DeletePropertySet, propertySet.IFCPropertySetDefinition));
            }
        }
        #region Комманды 

        #region Переименовать набороы

        public ICommand ReplaceNamePropertySetCommand { get; }

        private void OnReplaceNamePropertySetCommandExecuted(object o)
        {
            object[]? arrParameters =  o as object[];
            string? targetString = Convert.ToString(arrParameters[0]);
            string? filterString = Convert.ToString(arrParameters[1]);

            bool setFragment = (bool)arrParameters[2];
            bool setWhole = (bool)arrParameters[3];
            bool setPrefix = (bool)arrParameters[4];

            if (arrParameters[5] is IEnumerable targetPropertySetsSelect)
            {
                _targetObjects[0].ModelIFC.ChangeName(GetCollectionForChangeName(targetPropertySetsSelect, targetString, filterString, setFragment, setWhole, setPrefix));
            }

            ResetSearchConditions();
        }


        private bool CanReplaceNamePropertySetCommandExecute(object o)
        {
            return FilteredSearchItems != null && FilteredSearchItems.Count() > 0;
        }

        #endregion Переименовать набороы

        #region Удалить наборы

        public ICommand DeleteElementCommand { get; }

        private void OnDeleteElementCommandExecuted(object o)
        {
            if (o is IEnumerable targetPropertySetsSelect)
            {
                _targetObjects[0].ModelIFC.ActionInTransactionForPropertySet(GetPropertySetsToDelete(targetPropertySetsSelect));
            }
            ResetSearchConditions();       
        }

        private bool CanDeleteElementCommandExecute(object o)
        {
            return FilteredSearchItems != null && FilteredSearchItems.Count() > 0;
        }

        #endregion Удалить наборы



        #endregion Комманды

        public GroupEditPropertySetViewModel()
        {}

        public GroupEditPropertySetViewModel(IEnumerable<ModelItemIFCObject> modelElementsForSearch)
        {
            _targetObjects = new ObservableCollection<ModelItemIFCObject>(modelElementsForSearch);

            ResetSearchConditions();

            #region Комманды

            ReplaceNamePropertySetCommand = new ActionCommand(
                OnReplaceNamePropertySetCommandExecuted,
                CanReplaceNamePropertySetCommandExecute);

            DeleteElementCommand = new ActionCommand(
                OnDeleteElementCommandExecuted,
                CanDeleteElementCommandExecute);



            #endregion Комманды
        }
    }
}