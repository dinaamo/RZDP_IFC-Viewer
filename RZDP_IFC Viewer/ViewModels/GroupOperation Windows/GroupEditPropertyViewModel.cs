using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Editor_IFC;
using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json.Linq;
using RZDP_IFC_Viewer.IFC.Model.ModelObjectPropertySet.Base;
using RZDP_IFC_Viewer.IFC.ModelItem;
using RZDP_IFC_Viewer.Infracrucrure.Commands;
using RZDP_IFC_Viewer.ViewModels.Base;
using Xbim.Common;
using Xbim.Ifc4.Interfaces;

namespace RZDP_IFC_Viewer.ViewModels
{
    internal class GroupEditPropertyViewModel : BaseViewModel
    {
        private List<ModelItemIFCObject> _targetModelObjects;

        private ObservableCollection<IPropertyModel<IIfcResourceObjectSelect>> _FilteredSearchItems;

        public ObservableCollection<IPropertyModel<IIfcResourceObjectSelect>> FilteredSearchItems
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
            FilteredSearchItems = new(_targetModelObjects.SelectMany(it => it.CollectionPropertySet).
                Distinct().
                SelectMany(it => it.PropertyCollection));
        }

        #endregion Очистить фильтр

        public void Search(string nameProperty, string valueStringProperty)
        {
            FilteredSearchItems = new(_targetModelObjects.SelectMany(it => it.CollectionPropertySet).Distinct().
                                                                SelectMany(it => it.PropertyCollection).
                                                                    Where(it => it.NameProperty.Contains(nameProperty, StringComparison.OrdinalIgnoreCase)).
                                                                    Where(it => it.ValueString.Contains(valueStringProperty, StringComparison.OrdinalIgnoreCase)));
        }


        IEnumerable<(object, string)> GetCollectionForChangeName(IEnumerable targetProperties, string setString, string searchingString, bool setFragment, bool setWhole, bool setPrefix)
        {
            foreach (IPropertyModel<IIfcResourceObjectSelect> property in targetProperties)
            {
                string resultName = property.NameProperty;

                if (setFragment)
                {
                    if (string.IsNullOrEmpty(searchingString))
                        continue;
                    resultName = property.NameProperty.Replace(searchingString, setString, StringComparison.OrdinalIgnoreCase);
                }
                else if (setWhole)
                {
                    resultName = setString;
                }
                else if (setPrefix)
                {
                    resultName = setString + property.NameProperty;
                }
                yield return (property.Property, resultName);
                property.OnPropertyChanged("NameProperty");
            }
        }

        IEnumerable<(Action<string>, string)> GetCollectionForChangeValue(IEnumerable targetProperties, string setString, string searchingString, bool setFragment, bool setWhole)
        {
            foreach (IPropertyModel<IIfcResourceObjectSelect> property in targetProperties)
            {
                string resultName = property.ValueString;

                if (setFragment)
                {
                    if (string.IsNullOrEmpty(searchingString))
                        continue;
                    resultName = property.ValueString.Replace(searchingString, setString, StringComparison.OrdinalIgnoreCase);
                }
                else if (setWhole)
                {
                    resultName = setString;
                }
                yield return (property.SetNewValue, resultName);
                property.OnPropertyChanged("ValueString");
            }
        }

        IEnumerable<IPersistEntity> GetPropertyToDelete(IEnumerable targetPropertySelect)
        {
            List<IPersistEntity> list = new();
            foreach (IPropertyModel<IIfcResourceObjectSelect> property in targetPropertySelect)
            {
                if (!list.Any(it => it == property.Property))
                {
                    yield return property.Property;
                }
                list.Add(property.Property);
            }
        }
        #region Комманды 

        #region Переименовать параметр

        public ICommand ReplaceNamePropertyCommand { get; }

        private void OnReplaceNamePropertyCommandExecuted(object o)
        {
            object[]? arrParameters = o as object[];
            string? searchingString = Convert.ToString(arrParameters[0]);
            string? setString = Convert.ToString(arrParameters[1]);

            bool setFragment = (bool)arrParameters[2];
            bool setWhole = (bool)arrParameters[3];
            bool setPrefix = (bool)arrParameters[4];

            if (arrParameters[5] is IEnumerable targetPropertySetsSelect)
            {
                _targetModelObjects[0].ModelIFC.ChangeName(GetCollectionForChangeName(targetPropertySetsSelect, setString, searchingString, setFragment, setWhole, setPrefix));
            }


            //ResetSearchConditions();
        }


        private bool CanReplaceNamePropertyCommandExecute(object o)
        {
            return FilteredSearchItems != null && FilteredSearchItems.Count() > 0;
        }

        #endregion Переименовать набороы

        #region Измменить значение

        public ICommand ChangeValuePropertyCommand { get; }

        private void OnChangeValuePropertyommandExecuted(object o)
        {
            object[]? arrParameters = o as object[];
            string? searchingString = Convert.ToString(arrParameters[0]);
            string? setString = Convert.ToString(arrParameters[1]);

            bool setFragment = (bool)arrParameters[2];
            bool setWhole = (bool)arrParameters[3];

            if (arrParameters[4] is IEnumerable targetPropertySelect)
            {
                _targetModelObjects[0].ModelIFC.ChangeValue(GetCollectionForChangeValue(targetPropertySelect, setString, searchingString, setFragment, setWhole));
            }
            //OnPropertyChanged("ValueString");
            //ResetSearchConditions();
        }


        private bool CanChangeValuePropertyommandExecute(object o)
        {
            return FilteredSearchItems != null && FilteredSearchItems.Count() > 0;
        }

        #endregion Измменить значение

        #region Удалить параметры

        public ICommand DeleteElementCommand { get; }

        private void OnDeleteElementCommandExecuted(object o)
        {
            if (o is IEnumerable targetPropertySetsSelect)
            {
                _targetModelObjects[0].ModelIFC.DeleteIFCEntity(GetPropertyToDelete(targetPropertySetsSelect));
            }
            ResetSearchConditions();
        }

        private bool CanDeleteElementCommandExecute(object o)
        {
            return FilteredSearchItems != null && FilteredSearchItems.Count() > 0;
        }

        #endregion Удалить наборы



        #endregion Комманды

        public GroupEditPropertyViewModel()
        {}

        public GroupEditPropertyViewModel(IEnumerable<ModelItemIFCObject> modelElementsForSearch)
        {
            _targetModelObjects = new (modelElementsForSearch);

            ResetSearchConditions();

            #region Комманды

            ReplaceNamePropertyCommand = new ActionCommand(
                OnReplaceNamePropertyCommandExecuted,
                CanReplaceNamePropertyCommandExecute);

            ChangeValuePropertyCommand = new ActionCommand(
                OnChangeValuePropertyommandExecuted,
                CanChangeValuePropertyommandExecute);

            DeleteElementCommand = new ActionCommand(
                OnDeleteElementCommandExecuted,
                CanDeleteElementCommandExecute);



            #endregion Комманды
        }
    }
}