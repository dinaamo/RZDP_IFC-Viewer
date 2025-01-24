using System.Data;
using System.Windows.Input;
using RZDP_IFC_Viewer.IFC.Model;
using RZDP_IFC_Viewer.Infracrucrure.Commands;
using Xbim.Common;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

namespace RZDP_IFC_Viewer.IFC.ModelItem
{
    public abstract class BaseModelReferenceIFC : BaseModelItemIFC
    {
        public BaseModelReferenceIFC(IfcStore ifcStore, IIfcObjectReferenceSelect ifcObjectReferenceSelect, ModelIFC modelIFC) : base(modelIFC, ifcObjectReferenceSelect)
        {
            DeleteReferenceCommand = new ActionCommand(
                        OnDeleteReferenceCommandExecuted,
                        CanDeleteReferenceCommandExecute);

            OpenCommand = new ActionCommand(
                        OnOpenCommandExecuted,
                        CanOpenCommandExecute);

            this.ifcObjectReferenceSelect = ifcObjectReferenceSelect;


        }

        private IIfcObjectReferenceSelect ifcObjectReferenceSelect;

        public ICommand DeleteReferenceCommand { get; }

        protected abstract void OnDeleteReferenceCommandExecuted(object o);

        
        protected bool CanDeleteReferenceCommandExecute(object o)
        {
            if (Model == null)
            {
                return false;
            }
            else if (o is BaseModelReferenceIFC)
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        public ICommand OpenCommand { get; }

        protected abstract void OnOpenCommandExecuted(object o);


        protected bool CanOpenCommandExecute(object o)
        {
            if (Model == null)
            {
                return false;
            }
            else if (o is BaseModelReferenceIFC)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public abstract string NameReference { get; }

        public HashSet<object> ReferenceObjectCollection { get; set; }

        public virtual IIfcObjectReferenceSelect GetReference()
        {
            return ifcObjectReferenceSelect;
        }

        /// <summary>
        /// Коллекция ссылок на объекты
        /// </summary>
        ///
        private Dictionary<string, HashSet<object>> _PropertyElement;

        public override Dictionary<string, HashSet<object>> PropertyElement
        {
            get
            {
                if (_PropertyElement == null)
                {
                    _PropertyElement = new Dictionary<string, HashSet<object>>();
                }
                return _PropertyElement;
            }
            protected set
            {
                _PropertyElement = value;
            }
        }

        public void AddReferenceToTheElement(ModelItemIFCObject referenceObject)
        {
            if (!PropertyElement.ContainsKey("Ссылки на объекты"))
            {
                ReferenceObjectCollection = new HashSet<object>();
                PropertyElement.Add("Ссылки на объекты", ReferenceObjectCollection);
            }
            ReferenceObjectCollection.Add(referenceObject);
        }

        public void DeleteReferenceToTheElement(ModelItemIFCObject deleteReferenceObject)
        {
            PropertyElement["Ссылки на объекты"].Remove(deleteReferenceObject);
        }
    }
}