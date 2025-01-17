using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using IFC_Table_View.IFC.Model;
using IFC_Table_View.IFC.ModelItem;
using IFC_Table_View.Infracrucrure.Commands;
using RZDP_IFC_Viewer.IFC.Editor.Base;
using RZDP_IFC_Viewer.IFC.Model.ModelObjectPropertySet.Base;
using Xbim.Ifc4.Interfaces;

namespace Editor_IFC
{
    public abstract class BasePropertySetDefinition : BaseModel, INotifyPropertyChanged
    {
        private IIfcObjectDefinition ifcObjectDefinition;
        public IIfcPropertySetDefinition IFCPropertySetDefinition { get; }
        protected ModelItemIFCObject ModelObject { get; }



        //#region Показать элемент

        //public ICommand DeletePropertySetCommand { get; }

        //private void OnDeletePropertySetCommandExecuted(object o)
        //{
        //    if (ModelObject.DeletePropertySet(this))
        //    {

        //    }
        //}

        //private bool CanDeletePropertySetCommandExecute(object o)
        //{
        //    return true;
        //}

        //#endregion Показать элемент


        //public bool DeletePropertySet()
        //{
            
        //}

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
            }
        }

        protected BasePropertySetDefinition(IIfcObjectDefinition ifcObjectDefinition, IIfcPropertySetDefinition ifcPropertySetDef, ModelIFC modelIFC, ModelItemIFCObject modelObject) : base(modelIFC)
        {
            this.ifcObjectDefinition = ifcObjectDefinition;
            this.IFCPropertySetDefinition = ifcPropertySetDef;
            this.ModelObject = modelObject;

            //DeletePropertySetCommand = new ActionCommand(
            //       OnDeletePropertySetCommandExecuted,
            //       CanDeletePropertySetCommandExecute);
        }

        protected abstract IEnumerable<IPropertyModel<IIfcResourceObjectSelect>> FillCollectionProperty();

        //private ObservableCollection<IPropertyModel<IIfcResourceObjectSelect>> _PropertyCollection;

        public List<IPropertyModel<IIfcResourceObjectSelect>> PropertyCollection => FillCollectionProperty().ToList();
        //{
        //    get
        //    {
        //        if (_PropertyCollection is null)
        //        {
        //            _PropertyCollection = new ObservableCollection<IPropertyModel<IIfcResourceObjectSelect>>(FillCollectionProperty());
        //        }
        //        return _PropertyCollection;
        //    }
        //}

        public void UnpinPropertySet()
        {
            foreach (IIfcRelDefinesByProperties RelDef in IFCPropertySetDefinition.DefinesOccurrence)
            {
                RelDef.RelatedObjects.Remove(ifcObjectDefinition);
            }
        }

        private int DeterminingRelatedObjectsCount()
        {
            int RelatedObjectsCount = default;

            RelatedObjectsCount += IFCPropertySetDefinition?.DefinesOccurrence.SelectMany(it => it.RelatedObjects).Count() ?? 0;

            RelatedObjectsCount += IFCPropertySetDefinition?.DefinesType.Count() ?? 0;

            return RelatedObjectsCount;
        }

        public string NamePropertySet
        {
            get
            {
                return IFCPropertySetDefinition.Name;
            }
            set
            {
                ModelIFC.ChangeName(new List<(IIfcRoot, string)> { (IFCPropertySetDefinition, value) });
                OnPropertyChanged("NamePropertySet");
            }
        }

        public static BasePropertySetDefinition CreateEditorPropertySet(IIfcObjectDefinition ifcObjectDefinition,
                                IIfcPropertySetDefinition ifcPropertySetDefinition, ModelIFC modelIFC, ModelItemIFCObject modelItemIFCObject)
        {
            if (ifcPropertySetDefinition is Xbim.Ifc2x3.Kernel.IfcPropertySet ifcPropertySet2x3)
            {
                return new EdPropertySet2x3(ifcObjectDefinition, ifcPropertySet2x3, modelIFC, modelItemIFCObject);
            }
            else if (ifcPropertySetDefinition is Xbim.Ifc2x3.ProductExtension.IfcElementQuantity ifcElementQuantity2x3)
            {
                return new EdElementQuantity2x3(ifcObjectDefinition, ifcElementQuantity2x3, modelIFC, modelItemIFCObject);
            }
            else if (ifcPropertySetDefinition is Xbim.Ifc4.Kernel.IfcPropertySet ifcPropertySet4)
            {
                return new EdPropertySet4(ifcObjectDefinition, ifcPropertySet4, modelIFC, modelItemIFCObject);
            }
            else if (ifcPropertySetDefinition is Xbim.Ifc4.ProductExtension.IfcElementQuantity ifcElementQuantity4)
            {
                return new EdElementQuantity4(ifcObjectDefinition, ifcElementQuantity4, modelIFC, modelItemIFCObject);
            }
            else
            {
                throw new ArgumentException($"Не соответствие схемы ifc для набора характеристик");
            }
        }

        public abstract void AddProperty(string nameProperty, string valueProperty);

        public abstract void AddProperty();

        public abstract IIfcPropertySetDefinition GetCopyPropertySet();
    }
}