using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using IFC_Table_View.IFC.ModelItem;
using IFC_Table_View.Infracrucrure.Commands;
using IFC_Table_View.ViewModels.Base;

namespace IFC_Table_View.ViewModels
{
    internal class SelectReferenceObjectWindowViewModel : BaseViewModel
    {
        private Action<List<ModelItemIFCObject>, List<BaseModelReferenceIFC>> CreateNewModelReference;

        public List<BaseModelReferenceIFC> CollectionRefObjectToSelect { get; set; }

        private ObservableCollection<ItemSelect> _TableCollection;

        public ObservableCollection<ItemSelect> TableCollection
        {
            get
            {
                return _TableCollection;
            }

            private set
            {
                Set(ref _TableCollection, value);
            }
        }

        private ObservableCollection<ItemSelect> _DocumentCollection;

        public ObservableCollection<ItemSelect> DocumentCollection
        {
            get
            {
                return _DocumentCollection;
            }

            private set
            {
                Set(ref _DocumentCollection, value);
            }
        }

        private ObservableCollection<ModelItemIFCObject> _ObjectCollection;

        public ObservableCollection<ModelItemIFCObject> ObjectCollection
        {
            get
            {
                return _ObjectCollection;
            }

            private set
            {
                Set(ref _ObjectCollection, value);
            }
        }

        #region Комманды

        #region Задать ссылки

        public ICommand SetReference { get; }

        private void OnSetReferenceCommandExecuted(object o)
        {
            object[] ControlArray = (object[])o;
            DataGrid dgTables = ControlArray[0] as DataGrid;
            DataGrid dgDocument = ControlArray[1] as DataGrid;

            foreach (ItemSelect itemSelect in dgTables.Items)
            {
                if (itemSelect.IsSelect)
                {
                    CollectionRefObjectToSelect.Add(itemSelect.ModelReferenceIFC);
                }
            }
            foreach (ItemSelect itemSelect in dgDocument.Items)
            {
                if (itemSelect.IsSelect)
                {
                    CollectionRefObjectToSelect.Add(itemSelect.ModelReferenceIFC);
                }
            }

            CreateNewModelReference(ObjectCollection.ToList(), CollectionRefObjectToSelect);
        }

        private bool CanSetReferenceCommandExecute(object o)
        {
            return true;
        }

        #endregion Задать ссылки

        #endregion Комманды

        public SelectReferenceObjectWindowViewModel()
        {
        }

        public SelectReferenceObjectWindowViewModel(List<ModelItemIFCObject> collectionObject, List<BaseModelReferenceIFC> collectionModelReference, Action<List<ModelItemIFCObject>, List<BaseModelReferenceIFC>> createNewModelReference)
        {
            CreateNewModelReference = createNewModelReference;
            CollectionRefObjectToSelect = new List<BaseModelReferenceIFC>();
            ObjectCollection = new ObservableCollection<ModelItemIFCObject>(collectionObject);

            TableCollection = new ObservableCollection<ItemSelect>();
            foreach (BaseModelReferenceIFC item in collectionModelReference.OfType<ModelItemIFCTable>())
            {
                TableCollection.Add(new ItemSelect() { ModelReferenceIFC = item });
            }

            DocumentCollection = new ObservableCollection<ItemSelect>();
            foreach (BaseModelReferenceIFC item in collectionModelReference.OfType<ModelItemDocumentReference>())
            {
                DocumentCollection.Add(new ItemSelect() { ModelReferenceIFC = item });
            }

            #region Комманды

            SetReference = new ActionCommand(
                OnSetReferenceCommandExecuted,
                CanSetReferenceCommandExecute);

            #endregion Комманды
        }

        public class ItemSelect : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            /// <summary>
            /// Событие изменения элемента
            /// </summary>
            /// <param name = "PropertyName" ></ param >
            protected virtual void OnPropertyChanged(string PropertyName = null)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
                }
            }

            private bool _IsSelect = false;

            public BaseModelReferenceIFC ModelReferenceIFC { get; set; }

            public bool IsSelect
            {
                get => _IsSelect;
                set
                {
                    _IsSelect = value;
                    OnPropertyChanged("IsSelect");
                }
            }
        }
    }
}