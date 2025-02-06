using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using RZDP_IFC_Viewer.IFC.Model;
using RZDP_IFC_Viewer.Infracrucrure;
using RZDP_IFC_Viewer.IFC.Editor.Base;
using Xbim.Common;

namespace RZDP_IFC_Viewer.IFC.ModelItem
{
    public abstract class BaseModelItemIFC : BaseItemModel, INotifyPropertyChanged
    {
        public ModelIFC Model { get; set; }

        public BaseModelItemIFC(ModelIFC modelIFC, IPersistEntity ItemIFC = null, BaseModelItemIFC TopElement = null) : base(modelIFC)
        {
            this.Model = modelIFC;
            this.ItemIFC = ItemIFC;

            if (TopElement != null)
            {
                PropertyExpandChanged += TopElement.ChangeExpandProperty;
            }
        }

        public string IFCClass => ItemIFC?.GetType().Name ?? "";

        public virtual ObservableCollection<BaseModelItemIFC> ModelItems { get; }

        //IPersistEntity _ItemTreeView;
        public IPersistEntity ItemIFC { get; set; }

        public abstract Dictionary<string, HashSet<object>> PropertyElement { get; protected set; }

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

        /// <summary>
        /// Раскрываем дерево если нашли элемент
        /// </summary>
        public event EventHandler<PropertyExpandChangedEventArg> PropertyExpandChanged;

        public void ChangeExpandProperty(object obj, PropertyExpandChangedEventArg e)
        {
            if (e.IsExpandTree)
            {
                if (!IsExpanded)
                {
                    IsExpanded = true;
                    ExpandOver();
                }
            }
        }

        private bool _IsExpanded { get; set; } = false;

        public bool IsExpanded
        {
            get { return _IsExpanded; }
            set
            {
                _IsExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }

        public void ExpandOver()
        {
            PropertyExpandChanged?.Invoke(this, new PropertyExpandChangedEventArg(true));
        }

        private bool _IsSelected { get; set; } = false;

        ///  <summary>
        ///  IsSelected
        ///  </summary>
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (value)
                {
                    FontWeight = FontWeights.Bold;
                }
                else
                {
                    FontWeight = FontWeights.Normal;
                }

                _IsSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        private FontWeight _FontWeight { get; set; } = FontWeights.Normal;

        ///  <summary>
        /// IsSelected
        /// </summary>
        public FontWeight FontWeight
        {
            get { return _FontWeight; }
            set
            {
                _FontWeight = value;
                OnPropertyChanged("FontWeight");
            }
        }

        private bool _IsFocusReference { get; set; } = false;

        /// <summary>
        /// Фокус элемента в дереве
        /// </summary>
        public bool IsFocusReference
        {
            get { return _IsFocusReference; }
            set
            {
                _IsFocusReference = value;
                OnPropertyChanged("IsFocusReference");
            }
        }

    }
}