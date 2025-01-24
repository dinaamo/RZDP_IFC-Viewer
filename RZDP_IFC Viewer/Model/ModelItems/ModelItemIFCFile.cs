using System.Collections.ObjectModel;
using RZDP_IFC_Viewer.IFC.Model;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

namespace RZDP_IFC_Viewer.IFC.ModelItem
{
    public class ModelItemIFCFile : BaseModelItemIFC
    {
        public ModelItemIFCFile(IfcStore ifcStore, IIfcProject Project, ModelIFC modelIFC) : base(modelIFC)
        {
            this.Project = Project;
            AddPropertyObject();
        }

        public IIfcProject Project { get; private set; }

        public string FileName => Model.FileName;

        //public override object ItemTreeView => FileName;

        ///// <summary>
        ///// IsExpanded
        ///// </summary>
        //private bool _IsExpanded { get; set; } = false;
        //public override bool IsExpanded
        //{
        //    get { return _IsExpanded; }
        //    set
        //    {
        //        _IsExpanded = value;
        //        OnPropertyChanged("IsExpanded");
        //    }
        //}

        public override Dictionary<string, HashSet<object>> PropertyElement
        {
            get
            {
                return _PropertyElement;
            }
            protected set
            {
                _PropertyElement = value;
            }
        }

        private Dictionary<string, HashSet<object>> _PropertyElement;

        private void AddPropertyObject()
        {
            _PropertyElement = new Dictionary<string, HashSet<object>>
            {
                { "Путь к файлу", new HashSet<object>() { Model.IfcStore.Header.FileName.Name } },
                { "Версия", new HashSet<object>() { Convert.ToString(Model.IfcStore.SchemaVersion) } },
                { "Время создания файла", new HashSet<object>() { Convert.ToString(Model.IfcStore.Header.TimeStamp) } },
                { "Приложение", new HashSet<object>() { Convert.ToString(Model.IfcStore.Header.CreatingApplication) } },
                { "Автор проекта", new HashSet<object>() { Convert.ToString(Project.OwnerHistory?.OwningUser?.ThePerson?.GivenName) }},
                { "Организация", new HashSet<object>() { Convert.ToString(Project.OwnerHistory?.OwningUser?.TheOrganization?.Name) }},
            };
        }

        private ObservableCollection<BaseModelItemIFC> _ModelItems;

        public override ObservableCollection<BaseModelItemIFC> ModelItems
        {
            get
            {
                if (_ModelItems == null)
                {
                    _ModelItems = new ObservableCollection<BaseModelItemIFC>();
                }
                return _ModelItems;
            }
        }
    }
}