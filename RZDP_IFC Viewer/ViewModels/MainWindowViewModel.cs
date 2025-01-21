using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using IFC_Table_View.IFC.Model;
using IFC_Table_View.IFC.ModelItem;
using IFC_Table_View.Infracrucrure.Commands;
using IFC_Table_View.View.Windows;
using IFC_Table_View.ViewModels.Base;
using IFC_Viewer.View.Windows;
using NuGet;
using Xbim.Common;
using Xbim.Ifc;
using Xbim.ModelGeometry.Scene;
using Xbim.Presentation;
using static Microsoft.Isam.Esent.Interop.EnumeratedColumn;
using static Xbim.Presentation.DrawingControl3D;

namespace IFC_Table_View.ViewModels
{
    internal class MainWindowViewModel : BaseViewModel
    {
        private BackgroundWorker worker;

        #region Свойства

        HashSet<IPersistEntity> _deleteEntity;

        #region Модель

        private ModelIFC _Model;

        public ModelIFC Model
        {
            get
            { return _Model; }
            set
            {
                Set(ref _Model, value);
                Title = _Model.FilePath;
                mainWindow.treeViewIFC.ItemsSource = Model.ModelItems;
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

                    }
                });
                

                ModelIFC tempModel = ModelIFC.Create(ifcStore, ZoomSelected, SelectElement,
                    worker, RefreshDCAfterDeleteEntity);

                if (tempModel != null)
                {
                    if (Model is not null)
                    {
                        Model.Dispose();
                    }
                    task.Wait();

                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        if (!ifcStore.GeometryStore.IsEmpty)
                        {
                            Model = tempModel;
                            mainWindow.WPFDrawingControl.ModelProvider.ObjectInstance = ifcStore;
                            _deleteEntity = new HashSet<IPersistEntity>();
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
                Application.Current.Dispatcher.BeginInvoke(() =>{IsEnableWindow = true;});
                ProgressValue = 0;
                worker.ReportProgress(0);
                worker.DoWork -= LoadModel;
            }
        }



        #endregion Загрузка модели

        #region Скрываем объекты после удаления
        void RefreshDCAfterDeleteEntity(IEnumerable<IPersistEntity> persistEntitySet)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                _deleteEntity.AddRange(persistEntitySet);
                mainWindow.WPFDrawingControl.DrawingControl.HiddenInstances = new List<IPersistEntity>(_deleteEntity);
                mainWindow.WPFDrawingControl.DrawingControl.ReloadModel(DrawingControl3D.ModelRefreshOptions.ViewPreserveCameraPosition);
            });
        }
        #endregion

        #region Закрыть файл

        public bool CloseApplication()
        {
            //if (Model is not null && Model.IsEditModel)
            //{
            //    MessageBoxResult result = MessageBox.Show("Сохранить изменения в модели?", "RZDP IFC Viewer!", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            //    if (result == MessageBoxResult.Cancel)
            //    {
            //        return false;
            //    }
            //    else if (result == MessageBoxResult.Yes)
            //    {
            //        await Task.Run(() => { SaveModelAsIFC(this, new DoWorkEventArgs(Model.FilePath)); });
            //    }
            //}
            return true;
        }
        #endregion

        #region Сохранение модели

        private void SaveModelAsIFC(object sender, DoWorkEventArgs args)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                IsEnableWindow = false;
            });

            var path = args.Argument as string;

            Model.SaveFile(path, worker.ReportProgress);

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

            Model.SaveAsXMLFile(path, worker.ReportProgress);

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

        private void ZoomSelected(IPersistEntity persistEntity)
        {
            _camChanged = false;
            mainWindow.WPFDrawingControl.DrawingControl.Viewport.Camera.Changed += Camera_Changed;
            mainWindow.WPFDrawingControl.DrawingControl.SelectedEntity = persistEntity;
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

        private void SelectElement(IPersistEntity persistEntity)
        {
            //SelectionChangedEventArgs a = new SelectionChangedEventArgs(
            //SelectedEntityChangedEvent,
            //        new[] { SelectedEntityProperty },
            //        new[] { persistEntity }
            //        );

            //DrawingControl_SelectedEntityChanged(this , a);

            mainWindow.WPFDrawingControl.DrawingControl.SelectedEntity = persistEntity;
        }

        private void DrawingControl_SelectedEntityChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }



        #endregion Выделить элемент

        #region Блокировка окна при загрузке
        private void Worker_DoWork(object? sender, DoWorkEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                IsEnableWindow = false;
            });
        }

        private void Worker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                IsEnableWindow = true;
            });
        }

        
        #endregion Блокировка окна при загрузке

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
            AddTableWindow tableForm = new AddTableWindow(Model.CreateNewIFCTable);
            tableForm.ShowDialog();
        }

        private bool CanAddIFCTableCommandExecute(object o)
        {
            if (Model != null)
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
            AddDocumentWindow addDocumentWindow = new AddDocumentWindow(Model.FilePath, Model.CreateNewIFCDocumentInformation);
            addDocumentWindow.ShowDialog();
        }

        private bool CanAddDocumentCommandExecute(object o)
        {
            if (Model != null)
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
            ModelItemIFCObject ifcProject = Model.ModelItems[0].ModelItems[0] as ModelItemIFCObject;

            List<ModelItemIFCObject> collectionModelObject = ModelItemIFCObject.FindPaintObjects(ifcProject);

            List<BaseModelReferenceIFC> collectionModelReference = Model.ModelItems[0].ModelItems.
                                                OfType<BaseModelReferenceIFC>().
                                                ToList();

            SelectReferenceObjectWindow.CreateSelectReferenceObjectWindow(collectionModelObject, collectionModelReference, Model.AddReferenceToTheObject);

        }

        private bool CanAddReferenceToTheElementsExecute(object o)
        {
            if (Model != null)
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
            ModelItemIFCObject ifcProject = Model.ModelItems[0].ModelItems[0] as ModelItemIFCObject;

            List<ModelItemIFCObject> collectionModelObject = ModelItemIFCObject.FindPaintObjects(ifcProject);

            List<BaseModelReferenceIFC> collectionModelReference = Model.ModelItems[0].ModelItems.
                                                OfType<BaseModelReferenceIFC>().
                                                ToList();

            SelectReferenceObjectWindow.CreateSelectReferenceObjectWindow(collectionModelObject, collectionModelReference, Model.DeleteReferenceToTheObject);

        }

        private bool CanDeleteReferenceToTheElementsExecute(object o)
        {
            if (Model != null)
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
            ModelItemIFCObject ifcProject = Model.ModelItems[0].ModelItems[0] as ModelItemIFCObject;
            ifcProject.ResetSearchCommand.Execute(ifcProject);
        }

        private bool CanRemovePaintCommandExecute(object o)
        {
            return Model is not null;
        }

        #endregion Убрать выделение

        #region Сохранить_файл

        public ICommand SaveFileCommand { get; }

        private void OnSaveFileCommandExecuted(object o)
        {
            worker.DoWork += SaveModelAsIFC;
            worker.RunWorkerAsync(Model.FilePath);
        }

        private bool CanSaveFileCommandExecute(object o)
        {
            if (Model != null)
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
            if (Model != null)
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
            if (Model != null)
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
            mainWindow.WPFDrawingControl.DrawingControl.SelectedEntityChanged += DrawingControl_SelectedEntityChanged;
            worker.ProgressChanged += ProgressChanged;
            worker.WorkerReportsProgress = true;

            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

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

            //CloseApplicationCommand = new ActionCommand(
            //    OnCloseApplicationCommandExecuted,
            //    CanCloseApplicationCommandExecute);

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