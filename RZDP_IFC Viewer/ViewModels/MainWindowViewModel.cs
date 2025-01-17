using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using IFC_Table_View.IFC.Model;
using IFC_Table_View.IFC.ModelItem;
using IFC_Table_View.Infracrucrure.Commands;
using IFC_Table_View.View.Windows;
using IFC_Table_View.ViewModels.Base;
using IFC_Viewer.View.Windows;
using Xbim.Ifc;
using Xbim.ModelGeometry.Scene;

namespace IFC_Table_View.ViewModels
{
    internal class MainWindowViewModel : BaseViewModel
    {
        private BackgroundWorker worker;

        #region Свойства

        #region Модель

        private ModelIFC _modelIFC;

        public ModelIFC modelIFC
        {
            get
            { return _modelIFC; }
            set
            {
                Set(ref _modelIFC, value);
                Title = _modelIFC.FilePath;
                mainWindow.treeViewIFC.ItemsSource = modelIFC.ModelItems;
            }
        }

        #endregion Модель

        #region ProgressValue

        private double _ProgressValue;

        public double ProgressValue
        {
            get
            {
                return _ProgressValue;
            }
            set
            {
                Set(ref _ProgressValue, value);
            }
        }

        #endregion ProgressValue

        #region StatusMsg

        private string _StatusMsg;

        public string StatusMsg
        {
            get
            {
                return _StatusMsg;
            }
            set
            {
                Set(ref _StatusMsg, value);
            }
        }

        #endregion StatusMsg

        #region Заголовок

        private string _Title;

        ///<summary>Заголовок окна</summary>
        public string Title
        {
            get => _Title;
            set => Set(ref _Title, value);
        }

        #endregion Заголовок

        #region Окно

        private bool _IsEnableWindow = true;

        ///<summary>Заголовок окна</summary>
        public bool IsEnableWindow
        {
            get => _IsEnableWindow;
            set => Set(ref _IsEnableWindow, value);
        }

        #endregion Окно

        #endregion Свойства

        #region Методы

        #region Загрузка модели

        private void ProgressChanged(object sender, ProgressChangedEventArgs args)
        {
            if (args.ProgressPercentage < 0 || args.ProgressPercentage > 100)
                return;

            Application.Current.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    ProgressValue = args.ProgressPercentage;
                    StatusMsg = (string)args.UserState;
                }));
        }

        private void LoadModel(object sender, DoWorkEventArgs args)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                IsEnableWindow = false;
            });
            try
            {
                //using ManualResetEvent signal = new ManualResetEvent(false);

                var path = args.Argument as string;

                IfcStore ifcStore = IfcStore.Open(path, null, null, worker.ReportProgress);

                Task task = Task.Run(() =>
                {
                    if (ifcStore.GeometryStore.IsEmpty)
                    {
                        var context = new Xbim3DModelContext(ifcStore);
                        context.CreateContext(worker.ReportProgress);
                        //signal.Set();
                    }
                });

                ModelIFC tempModel = ModelIFC.Create(ifcStore, ZoomSelected, SelectElement, worker.ReportProgress);

                if (tempModel != null)
                {
                    if (modelIFC is not null)
                    {
                        modelIFC.Dispose();
                    }
                    task.Wait();

                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        if (!ifcStore.GeometryStore.IsEmpty)
                        {
                            modelIFC = tempModel;
                            mainWindow.WPFDrawingControl.ModelProvider.ObjectInstance = ifcStore;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка {ex}", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ProgressValue = 0;
                worker.ReportProgress(0);
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    IsEnableWindow = true;
                });
                worker.DoWork -= LoadModel;
            }
        }

        #endregion Загрузка модели

        #region Сохранение модели

        private void SaveModelAsIFC(object sender, DoWorkEventArgs args)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                IsEnableWindow = false;
            });

            var path = args.Argument as string;

            modelIFC.SaveFile(path, worker.ReportProgress);

            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                IsEnableWindow = true;
            });
            worker.DoWork -= SaveModelAsIFC;
        }

        private void SaveModelAsIFCXML(object sender, DoWorkEventArgs args)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                IsEnableWindow = false;
            });

            var path = args.Argument as string;

            modelIFC.SaveAsXMLFile(path, worker.ReportProgress);

            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                IsEnableWindow = true;
            });
            worker.DoWork -= SaveModelAsIFCXML;
        }

        #endregion Сохранение модели

        #region Фокус на элемент

        private bool _simpleFastExtrusion = false;
        private bool _camChanged;

        private void ZoomSelected(ModelItemIFCObject modelItemIFCObject)
        {
            _camChanged = false;
            mainWindow.WPFDrawingControl.DrawingControl.Viewport.Camera.Changed += Camera_Changed;
            mainWindow.WPFDrawingControl.DrawingControl.SelectedEntity = modelItemIFCObject.ItemIFC;
            mainWindow.WPFDrawingControl.DrawingControl.ZoomSelected();
            mainWindow.WPFDrawingControl.DrawingControl.Viewport.Camera.Changed -= Camera_Changed;
            if (!_camChanged)
                mainWindow.WPFDrawingControl.DrawingControl.ClipBaseSelected(0.15);
        }

        private void Camera_Changed(object sender, EventArgs e)
        {
            _camChanged = true;
        }

        #endregion Фокус на элемент

        #region Выделить элемент

        private void SelectElement(ModelItemIFCObject modelItemIFCObject)
        {
            mainWindow.WPFDrawingControl.DrawingControl.SelectedEntity = modelItemIFCObject.ItemIFC;
        }

        #endregion Выделить элемент

        #endregion Методы

        #region Комманды

        #region Открыть_файл

        public ICommand LoadApplicationCommand { get; }

        private void OnLoadApplicationCommandExecuted(object o)
        {
            string path = o as string;

            if (string.IsNullOrEmpty(path))
            {
                path = HelperFileIFC.OpenIFC_File();
            }

            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            worker.DoWork += LoadModel;
            worker.RunWorkerAsync(path);
            //worker.DoWork -= LoadModel;
        }

        private bool CanLoadApplicationCommandExecute(object o)
        {
            return true;
        }

        #endregion Открыть_файл

        #region Добавить_таблицу

        public ICommand AddIFCTableCommand { get; }

        private void OnAddIFCTableCommandExecuted(object o)
        {
            AddTableWindow tableForm = new AddTableWindow(modelIFC.CreateNewIFCTable);
            tableForm.ShowDialog();
        }

        private bool CanAddIFCTableCommandExecute(object o)
        {
            if (modelIFC != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion Добавить_таблицу

        #region Добавить_документ

        public ICommand AddDocumentCommand { get; }

        private void OnAddDocumentCommandExecuted(object o)
        {
            AddDocumentWindow addDocumentWindow = new AddDocumentWindow(modelIFC.FilePath, modelIFC.CreateNewIFCDocumentInformation);
            addDocumentWindow.ShowDialog();
        }

        private bool CanAddDocumentCommandExecute(object o)
        {
            if (modelIFC != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion Добавить_документ

        #region Добавить к элементам связи с таблицами

        public ICommand AddReferenceToTheElements { get; }

        private void OnAddReferenceToTheElementsExecuted(object o)
        {
            ModelItemIFCObject ifcProject = modelIFC.ModelItems[0].ModelItems[0] as ModelItemIFCObject;

            List<ModelItemIFCObject> collectionModelObject = ModelItemIFCObject.FindPaintObjects(ifcProject);

            List<BaseModelReferenceIFC> collectionModelReference = modelIFC.ModelItems[0].ModelItems.
                                                OfType<BaseModelReferenceIFC>().
                                                ToList();

            SelectReferenceObjectWindow window_Add_Reference_To_Table = new SelectReferenceObjectWindow(collectionModelObject, collectionModelReference, modelIFC.AddReferenceToTheObject);

            window_Add_Reference_To_Table.ShowDialog();
        }

        private bool CanAddReferenceToTheElementsExecute(object o)
        {
            if (modelIFC != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion Добавить к элементам связи с таблицами

        #region Удалить от элементов связи с ссылочными элементами

        public ICommand DeleteReferenceToTheElements { get; }

        private void OnDeleteReferenceToTheElementsExecuted(object o)
        {
            ModelItemIFCObject ifcProject = modelIFC.ModelItems[0].ModelItems[0] as ModelItemIFCObject;

            List<ModelItemIFCObject> collectionModelObject = ModelItemIFCObject.FindPaintObjects(ifcProject);

            List<BaseModelReferenceIFC> collectionModelReference = modelIFC.ModelItems[0].ModelItems.
                                                OfType<BaseModelReferenceIFC>().
                                                ToList();

            SelectReferenceObjectWindow window_Add_Reference_To_Table = new SelectReferenceObjectWindow(collectionModelObject, collectionModelReference, modelIFC.DeleteReferenceToTheObject);

            window_Add_Reference_To_Table.ShowDialog();
        }

        private bool CanDeleteReferenceToTheElementsExecute(object o)
        {
            if (modelIFC != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion Удалить от элементов связи с ссылочными элементами

        #region Убрать выделение

        public ICommand RemovePaintCommand { get; }

        private void OnRemovePaintCommandExecuted(object o)
        {
            ModelItemIFCObject ifcProject = modelIFC.ModelItems[0].ModelItems[0] as ModelItemIFCObject;
            ifcProject.ResetSearchCommand.Execute(ifcProject);
        }

        private bool CanRemovePaintCommandExecute(object o)
        {
            return modelIFC is not null;
        }

        #endregion Убрать выделение

        #region Сохранить_файл

        public ICommand SaveFileCommand { get; }

        private void OnSaveFileCommandExecuted(object o)
        {
            worker.DoWork += SaveModelAsIFC;
            worker.RunWorkerAsync(modelIFC.FilePath);
        }

        private bool CanSaveFileCommandExecute(object o)
        {
            if (modelIFC != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion Сохранить_файл

        #region Сохранить_файл_как_ifc

        public ICommand SaveAsFileCommand { get; }

        private void OnSaveAsIFCFileCommandExecuted(object o)
        {
            string path = HelperFileIFC.SaveAsIFC_File("ifc");

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            worker.DoWork += SaveModelAsIFC;
            worker.RunWorkerAsync(path);
        }

        private bool CanSaveAsIFCFileCommandExecute(object o)
        {
            if (modelIFC != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion Сохранить_файл_как_ifc

        #region Сохранить_файл_как_ifcxml

        public ICommand SaveAsIFCXMLFileCommand { get; }

        private void OnSaveAsIFCXMLFileCommandExecuted(object o)
        {
            string path = HelperFileIFC.SaveAsIFC_File("ifcxml");

            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            worker.DoWork += SaveModelAsIFCXML;
            worker.RunWorkerAsync(path);
        }

        private bool CanSaveAsIFCXMLFileCommandExecute(object o)
        {
            if (modelIFC != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion Сохранить_файл_как_ifcxml

        #region Открыть справку

        public ICommand OpenHelp { get; }

        private void OnOpenHelpCommandExecuted(object o)
        {
            string fileHelpPath = "IFC Table View.chm";
            if (System.IO.File.Exists(fileHelpPath))
            {
                Process.Start(fileHelpPath);
            }
        }

        private bool CanOpenHelpCommandExecute(object o)
        {
            return true;
        }

        #endregion Открыть справку

        #region Действие с раскрывающимися списками

        public ICommand ActionExpanders { get; }

        private void OnActionExpandedCommandExecuted(object o)
        {
            if (IsExpandedPropertySet)
            {
                IsExpandedPropertySet = false;
            }
            else
            {
                IsExpandedPropertySet = true;
            }
        }

        private bool _IsExpandedPropertySet { get; set; }

        public bool IsExpandedPropertySet
        {
            get { return _IsExpandedPropertySet; }
            set
            {
                _IsExpandedPropertySet = value;
                OnPropertyChanged("IsExpandedPropertySet");
            }
        }

        private bool CanActionExpandedCommandExecute(object o)
        {
            return true;
        }

        #endregion Действие с раскрывающимися списками

        #endregion Комманды

        public MainWindowViewModel()
        {
        }

        private readonly MainWindow mainWindow;

        public MainWindowViewModel(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            //IsEnableWindow = true;
            worker = new BackgroundWorker();

            worker.ProgressChanged += ProgressChanged;
            worker.WorkerReportsProgress = true;

            var strArray = System.Environment.GetCommandLineArgs();
            if (strArray.Length > 1)
            {
                worker.DoWork += LoadModel;
                worker.RunWorkerAsync(Environment.GetCommandLineArgs()[1]);
                worker.DoWork += LoadModel;
            }

            #region Комманды

            LoadApplicationCommand = new ActionCommand(
                OnLoadApplicationCommandExecuted,
                CanLoadApplicationCommandExecute);

            AddIFCTableCommand = new ActionCommand(
                OnAddIFCTableCommandExecuted,
                CanAddIFCTableCommandExecute);

            AddDocumentCommand = new ActionCommand(
                OnAddDocumentCommandExecuted,
                CanAddDocumentCommandExecute);

            AddReferenceToTheElements = new ActionCommand(
                OnAddReferenceToTheElementsExecuted,
                CanAddReferenceToTheElementsExecute);

            DeleteReferenceToTheElements = new ActionCommand(
                OnDeleteReferenceToTheElementsExecuted,
                CanDeleteReferenceToTheElementsExecute);

            RemovePaintCommand = new ActionCommand(
                OnRemovePaintCommandExecuted,
                CanRemovePaintCommandExecute);

            SaveFileCommand = new ActionCommand(
                OnSaveFileCommandExecuted,
                CanSaveFileCommandExecute);

            SaveAsFileCommand = new ActionCommand(
                OnSaveAsIFCFileCommandExecuted,
                CanSaveAsIFCFileCommandExecute);

            SaveAsIFCXMLFileCommand = new ActionCommand(
                OnSaveAsIFCXMLFileCommandExecuted,
                CanSaveAsIFCXMLFileCommandExecute);

            OpenHelp = new ActionCommand(
                OnOpenHelpCommandExecuted,
                CanOpenHelpCommandExecute);

            ActionExpanders = new ActionCommand(
                OnActionExpandedCommandExecuted,
                CanActionExpandedCommandExecute);

            #endregion Комманды
        }
    }
}