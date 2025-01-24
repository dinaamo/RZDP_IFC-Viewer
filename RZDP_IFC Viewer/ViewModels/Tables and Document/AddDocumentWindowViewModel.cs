using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using IFC_Table_View.Infracrucrure.Commands;
using IFC_Table_View.ViewModels.Base;

namespace IFC_Table_View.ViewModels
{
    internal class AddDocumentWindowViewModel : BaseViewModel
    {
        public Action<ModelDocument> CreateNewIFCDocumentInformation;

        #region Выбрать документы

        public ICommand SelectDocumentCommand { get; }

        private void OnSelectDocumentCommandExecuted(object o)
        {
            foreach (string path in HelperFileIFC.SelectDocument())
            {
                DocumentCollection.Add(new ModelDocument(path, IFCPath));
            }
        }

        private bool CanSelectDocumentCommandExecute(object o)
        {
            return true;
        }

        #endregion Выбрать документы

        #region Очистить

        public ICommand ClearCommand { get; }

        private void OnClearCommandExecuted(object o)
        {
            DocumentCollection.Clear();
        }

        private bool CanClearCommandExecute(object o)
        {
            if (DocumentCollection != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion Очистить

        #region Удалить

        public ICommand DeleteDocumentCommand { get; }

        private void OnDeleteDocumentCommandExecuted(object o)
        {
            if (o is ModelDocument document)
            {
                DocumentCollection.Remove(document);
            }
        }

        private bool CanDeleteDocumentCommandExecute(object o)
        {
            if (o is ModelDocument document)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion Удалить

        #region Добавить в файл ссылку на документ

        public ICommand AddReferenceDocumentCommand { get; }

        private void OnAddReferenceDocumentCommandExecuted(object o)
        {
            foreach (ModelDocument document in DocumentCollection)
            {
                CreateNewIFCDocumentInformation(document);
            }
            DocumentCollection.Clear();
        }

        private bool CanAddReferenceDocumentCommandExecute(object o)
        {
            if (DocumentCollection.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion Добавить в файл ссылку на документ

        public ObservableCollection<ModelDocument> DocumentCollection
        {
            get
            {
                if (_DocumentCollection == null)
                {
                    _DocumentCollection = new ObservableCollection<ModelDocument>();
                }
                return _DocumentCollection;
            }
            private set
            {
                Set(ref _DocumentCollection, value);
            }
        }

        private ObservableCollection<ModelDocument> _DocumentCollection;

        public string _IFCPath;

        public string IFCPath
        {
            get
            {
                return _IFCPath;
            }
            set
            {
                Set(ref _IFCPath, value);
            }
        }

        public AddDocumentWindowViewModel()
        {
            SelectDocumentCommand = new ActionCommand(
                OnSelectDocumentCommandExecuted,
                CanSelectDocumentCommandExecute);

            ClearCommand = new ActionCommand(
                OnClearCommandExecuted,
                CanClearCommandExecute);

            DeleteDocumentCommand = new ActionCommand(
                OnDeleteDocumentCommandExecuted,
                CanDeleteDocumentCommandExecute);

            AddReferenceDocumentCommand = new ActionCommand(
                OnAddReferenceDocumentCommandExecuted,
                CanAddReferenceDocumentCommandExecute);
        }
    }

    public class ModelDocument
    {
        public ModelDocument(string path, string ifcPath)
        {
            NameDocument = Path.GetFileName(path);
            FullPath = path;
            //RelativePath = GetRelativePath(path, ifcPath);

            CreationTime = File.GetCreationTime(path);
            LastRevisionTime = File.GetLastWriteTime(path);
            ElectronicFormat = Path.GetExtension(path);

            Uri path1 = new Uri(ifcPath);
            Uri path2 = new Uri(path);
            Uri diff = path1.MakeRelativeUri(path2);
            string relPath = diff.ToString();

            RelativePath = relPath.Replace(@"/", @"\");
        }

        public string NameDocument { get; set; }
        public string FullPath { get; set; }

        public string RelativePath { get; set; }

        public string StringCreationTime
        { get { return CreationTime.ToString("D"); } }

        public string StringLastRevisionTime
        { get { return LastRevisionTime.ToString("D"); } }

        public DateTime CreationTime { get; set; }

        public DateTime LastRevisionTime { get; set; }

        public string ElectronicFormat { get; set; }
    }
}