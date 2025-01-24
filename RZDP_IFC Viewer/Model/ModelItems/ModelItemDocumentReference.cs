using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using RZDP_IFC_Viewer.IFC.Model;
using RZDP_IFC_Viewer.View.Windows;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

namespace RZDP_IFC_Viewer.IFC.ModelItem
{
    public class ModelItemDocumentReference : BaseModelReferenceIFC
    {
        public ModelItemDocumentReference(IfcStore ifcStore, IIfcDocumentReference ifcDocumentReference, ModelIFC modelIFC) : base(ifcStore, ifcDocumentReference, modelIFC)
        {
            this.ifcDocumentReference = ifcDocumentReference;

            FullPath = GetFullPath(modelIFC.FileName, modelIFC.FilePath, ReferencePath);

            _PropertyFile = new Dictionary<string, HashSet<object>>();

            IsFileExists = FileExist(FullPath);
        }

        #region Свойства

        private IIfcDocumentReference ifcDocumentReference;

        public override string NameReference => ifcDocumentReference.Name;

        private string FullPath { get; }
        private string ReferencePath => ifcDocumentReference.ReferencedDocument?.Identification;

        private bool FileExist(string FullPath)
        {
            if (File.Exists(FullPath))
            {
                PropertyFile.Clear();
                AddProppertyDocument();
                IsFileExists = true;
            }
            else
            {
                IsFileExists = false;
            }
            return IsFileExists;
        }

        /// <summary>
        /// IsExpanded
        /// </summary>
        private bool _IsFileExists { get; set; } = false;

        public bool IsFileExists
        {
            get { return _IsFileExists; }
            set
            {
                _IsFileExists = value;
                OnPropertyChanged("IsFileExists");
            }
        }

        /// <summary>
        /// Коллекция ссылок на объекты
        /// </summary>
        ///
        private Dictionary<string, HashSet<object>> _PropertyFile;

        public Dictionary<string, HashSet<object>> PropertyFile
        {
            get
            {
                return _PropertyFile;
            }
            protected set
            {
                _PropertyFile = value;
                OnPropertyChanged("PropertyFile");
            }
        }

        #endregion Свойства

        #region Методы

        public void OpenDocument()
        {
            if (FileExist(FullPath))
            {
                Process p = new Process();
                p.StartInfo = new ProcessStartInfo(FullPath)
                {
                    UseShellExecute = true
                };
                p.Start();
            }
            else
            {
                MessageBox.Show("Файл не найден", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AddProppertyDocument()
        {
            PropertyFile.Add("Имя", new HashSet<object>() { ifcDocumentReference.Name });
            PropertyFile.Add("Предполагаемое расположение документа", new HashSet<object>() { FullPath });
            PropertyFile.Add("Относительный путь", new HashSet<object>() { ReferencePath });
            PropertyFile.Add("Цель документа", new HashSet<object>() { ifcDocumentReference.ReferencedDocument?.Purpose });
            PropertyFile.Add("Предполагаемое использование документа", new HashSet<object>() { ifcDocumentReference.ReferencedDocument?.IntendedUse });
            PropertyFile.Add("Расширение", new HashSet<object>() { ifcDocumentReference.ReferencedDocument?.ElectronicFormat });
            PropertyFile.Add("Дата и время создания документа", new HashSet<object>() { File.GetCreationTime(FullPath) });
            PropertyFile.Add("Дата и время последнего изменения документа", new HashSet<object>() { File.GetLastWriteTime(FullPath) });
        }

        private string GetFullPath(string fileName, string fullIFCPath, string referencePath)
        {
            string separstor = @"\\";
            var PathToList = Regex.Split(fullIFCPath, separstor).ToList();

            PathToList.Remove(fileName);

            var tt = Regex.Matches(referencePath, @"\.\.\\", RegexOptions.RightToLeft);

            int countTopDirectories = tt.Count;

            PathToList.RemoveRange(PathToList.Count - countTopDirectories, countTopDirectories);

            var www = referencePath.Replace("..\\", "");
            PathToList.Add(www);

            string result = string.Join("\\", PathToList);
            return result;
        }

        #endregion Методы

        #region Комманды

        #region Удалить_ссылку на документ

        protected override void OnDeleteReferenceCommandExecuted(object o)
        {
            MessageBoxResult result = MessageBox.Show("Удалить ссылку на документ?", "Внимание!", MessageBoxButton.OKCancel, MessageBoxImage.Question);

            if (result != MessageBoxResult.OK)
            { return; }

            Model.DeleteIFCObjectReferenceSelect(this);
        }

        #endregion Удалить_ссылку на документ


        #region Открыть_документ
        protected override void OnOpenCommandExecuted(object o)
        {
            OpenDocument();
        }
        #endregion Открыть_документ
        #endregion Комманды
    }
}