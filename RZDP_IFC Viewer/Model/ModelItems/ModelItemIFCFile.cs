using System.Collections.ObjectModel;
using System.Windows.Input;
using RZDP_IFC_Viewer.IFC.Model;
using RZDP_IFC_Viewer.Infracrucrure.Commands;
using RZDP_IFC_Viewer.View.Windows;
using RZDP_IFC_Viewer.View.Windows.EditorWindows;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

namespace RZDP_IFC_Viewer.IFC.ModelItem
{
    public class ModelItemIFCFile : BaseModelItemIFC
    {
        public ModelItemIFCFile(IfcStore ifcStore, IIfcProject Project, ModelIFC modelIFC) : base(modelIFC)
        {
            this.Project = Project;
            GetPropertyObject();

            EditElementsCommand = new ActionCommand(
               OnEditElementsCommandExecuted,
               CanEditElementsCommandExecute);
        }

        public ModelItemIFCFile() :base(null){ }

        #region Свойства
        public IIfcProject Project { get; private set; }

        public string FileName => Model.FileName;

        public string Application
        {
            get
            {
                return Project.OwnerHistory.OwningApplication.ApplicationFullName;
            }
            set
            {
                Model.ChangeName(new List<(IIfcApplication, string)> { (Project.OwnerHistory.OwningApplication, value) });
                OnPropertyChanged("Application");
                OnPropertyChanged("PropertyElement");
                
            }
        }

        public string Person
        {
            get
            {
                return Project.OwnerHistory?.OwningUser.ThePerson?.GivenName;
            }
            set
            {
                Model.ChangeName(new List<(IIfcPerson, string)> { (Project.OwnerHistory?.OwningUser.ThePerson, value) });
                OnPropertyChanged("Person");
                OnPropertyChanged("PropertyElement");
            }
        }

        public string Organization
        {
            get
            {
                return Project.OwnerHistory?.OwningUser?.TheOrganization.Name;
            }
            set
            {
                Model.ChangeName(new List<(IIfcOrganization, string)> { (Project.OwnerHistory?.OwningUser?.TheOrganization, value) });
                OnPropertyChanged("Person");
                OnPropertyChanged("PropertyElement");
            }
        }

        #endregion

        #region Комманды

        #region Редактирование

        public ICommand EditElementsCommand { get; }

        private void OnEditElementsCommandExecuted(object o)
        {
            new FileEditorWindow(this).ShowDialog();
        }

        private bool CanEditElementsCommandExecute(object o)
        {
            return true;
        }

        #endregion Редактирование

        #endregion



        public override Dictionary<string, HashSet<object>> PropertyElement
        {
            get
            {
                return GetPropertyObject();
            }
            protected set
            {
                //_PropertyElement = value;
            }
        }

        //private Dictionary<string, HashSet<object>> _PropertyElement => GetPropertyObject();

        private Dictionary<string, HashSet<object>> GetPropertyObject()
        {
            return new Dictionary<string, HashSet<object>>
            {
                { "Путь к файлу", new HashSet<object>() { Model.IfcStore.Header.FileName.Name } },
                { "Версия", new HashSet<object>() { Convert.ToString(Model.IfcStore.SchemaVersion) } },
                { "Время создания файла", new HashSet<object>() { Convert.ToString(Model.IfcStore.Header.TimeStamp) } },
                { "Приложение", new HashSet<object>() { Convert.ToString(Project.OwnerHistory.OwningApplication.ApplicationFullName) } },
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