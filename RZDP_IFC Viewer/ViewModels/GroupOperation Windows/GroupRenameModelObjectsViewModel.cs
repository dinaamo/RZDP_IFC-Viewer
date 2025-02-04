using System.CodeDom;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Xml.Linq;
using Editor_IFC;
using Microsoft.Office.Interop.Excel;
using RZDP_IFC_Viewer.IFC.ModelItem;
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

        List<string> TargetParameters { get; set; }

        public string SelectedParameters {  get; set; }

        public ObservableCollection<ParameterForSetup> ParametersSelection { get; }

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

        IEnumerable<(IIfcRoot, string)> GetCollectionForSetNameFromParameters(IEnumerable targetModelObjects)
        {
            foreach (ModelItemIFCObject modelObject in targetModelObjects)
            {
                string newNameModelObject = string.Empty;

                var CollectionParameters = modelObject.CollectionPropertySet.
                                                            Distinct().
                                                            SelectMany(it => it.PropertyCollection).
                                                            Where(it => it.DataType == "String");
                foreach (string val in TargetParameters)
                {
                    var searchingParameter = CollectionParameters.FirstOrDefault(it => it.NameProperty == val);
                    if (searchingParameter != null)
                    {
                        newNameModelObject += searchingParameter.ValueString;
                        newNameModelObject += " ";
                    }
                }
                if (newNameModelObject == string.Empty)
                {
                    continue;
                }

                newNameModelObject = Regex.Replace(newNameModelObject, @"\s+", " ");
                newNameModelObject = newNameModelObject.Trim((char)32);

                yield return (modelObject.GetIFCObjectDefinition(), newNameModelObject.ToString());
                modelObject.OnPropertyChanged("IFCObjectName");
            }
        }


        IEnumerable<(IIfcRoot, string)> GetCollectionForChangeName(IEnumerable targetModelObjects, string setValueString, string searchingString, bool setFragment, bool setWhole, bool setPrefix)
        {
            foreach (ModelItemIFCObject modelObject in targetModelObjects)
            {
                string resultName = modelObject.IFCObjectName;

                if (setFragment)
                {
                    if (string.IsNullOrEmpty(searchingString))
                        continue;
                    resultName = modelObject.IFCObjectName.Replace(searchingString, setValueString, StringComparison.OrdinalIgnoreCase);
                }
                else if (setWhole)
                {
                    resultName = setValueString;
                }
                else if (setPrefix)
                {
                    resultName = setValueString + modelObject.IFCObjectName;
                }
                yield return (modelObject.GetIFCObjectDefinition(), resultName);
                modelObject.OnPropertyChanged("IFCObjectName");
            }
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

            if (arrParameters[5] is IEnumerable targetModelObjects)
            {
                FilteredSearchItems[0].Model.ChangeName(GetCollectionForChangeName(
                                                                        targetModelObjects, 
                                                                        setValueString, 
                                                                        searchingString,
                                                                        setFragment,
                                                                        setWhole,
                                                                        setPrefix));
            }
        }


        private bool CanReplaceNameModelObjectCommandExecute(object o)
        {
            return FilteredSearchItems.Count>0;
        }

        #endregion Переименовать элементы

        #region Задать имя из параметра

        public ICommand SetNameFromParameterCommand { get; }

        private void OnSetNameFromParameterCommandExecuted(object o)
        {
            if (o is IEnumerable targetPropertySetsSelect)
            {
                FilteredSearchItems[0].Model.ChangeName(GetCollectionForSetNameFromParameters(targetPropertySetsSelect));
            }
        }

        private bool CanSetNameFromParameterCommandExecute(object o)
        {
            return FilteredSearchItems.Count > 0;
        }

        #endregion Задать имя из параметра


        #region Выбор ячейки

        public ICommand CheckedUncheckedCommand { get; }

        private void OnCheckedUncheckedCommandExecuted(object o)
        {
            if (o is ParameterForSetup ParameterForSetup)
            {
                if (ParameterForSetup.IsSelected)
                {
                    TargetParameters.Add(ParameterForSetup.Name);
                }
                else if (!ParameterForSetup.IsSelected)
                {
                    TargetParameters.Remove(ParameterForSetup.Name);
                }
                SelectedParameters = string.Join("; ", TargetParameters.ToArray());
                OnPropertyChanged("SelectedParameters");
            }
        }

        private bool CanCheckedUncheckedCommandExecute(object o)
        {
            return true;
        }

        #endregion Выбор ячейки



        #endregion Комманды

        public GroupRenameModelObjectsViewModel()
        {}

        public GroupRenameModelObjectsViewModel(IEnumerable<ModelItemIFCObject> modelElementsForSearch)
        {
            TargetModelObjects = new(modelElementsForSearch);

            ResetSearchConditions();

            TargetParameters = new();

            ParametersSelection = new (TargetModelObjects.SelectMany(it => it.CollectionPropertySet).
                                                            Distinct().
                                                            SelectMany(it => it.PropertyCollection).
                                                            Where(it => it.DataType == "String").
                                                            Select(it => it.NameProperty).
                                                            Distinct().
                                                            Select(it => new ParameterForSetup(it)));
            
            #region Комманды

            ReplaceNameModelObjectCommand = new ActionCommand(
                OnReplaceNameModelObjectCommandExecuted,
                CanReplaceNameModelObjectCommandExecute);

            SetNameFromParameterCommand = new ActionCommand(
                OnSetNameFromParameterCommandExecuted,
                CanSetNameFromParameterCommandExecute);

            CheckedUncheckedCommand = new ActionCommand(
                OnCheckedUncheckedCommandExecuted,
                CanCheckedUncheckedCommandExecute);

            #endregion Комманды
        }
    }

    class ParameterForSetup : INotifyPropertyChanged
    {
        public ParameterForSetup(string name)
        {
            Name=name;
        }
        public string Name { get; }
        private bool _IsSelected;
        public bool IsSelected 
        {
            get
            {
                return _IsSelected;
            }
            set 
            { 
                _IsSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Событие изменения элемента
        /// </summary>
        /// <param name = "PropertyName" ></ param >
        public virtual void OnPropertyChanged(string PropertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
            }
        }
    }
}