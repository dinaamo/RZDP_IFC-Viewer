using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using RZDP_IFC_Viewer.DWG;
using IFC_Viewer.View.Windows;
using RZDP_IFC_Viewer.IFC.Model;
using RZDP_IFC_Viewer.IFC.ModelItem;
using RZDP_IFC_Viewer.Infracrucrure.Commands;
using RZDP_IFC_Viewer.View.Windows;
using RZDP_IFC_Viewer.ViewModels.Base;
using Tedd;
using Xbim.Common;
using Xbim.Ifc;
using Xbim.IO;
using Xbim.ModelGeometry.Scene;
using Xbim.Presentation;
using Xbim.Common.Geometry;
using Xbim.Ifc4.Interfaces;

namespace RZDP_IFC_Viewer.ViewModels
{
    

    internal class MainWindowViewModel : BaseViewModel
    {
        private BackgroundWorker _worker;
        private ManualResetEvent _signal;
        HashSet<IPersistEntity> _deleteEntity;
        HashSet<IPersistEntity> _hideEntity;
        private readonly MainWindow mainWindow;
        private DrawingControl3D _DrawingControl { get { return mainWindow.WPFDrawingControl.DrawingControl; } }
        private Vector3D _vectorOffset;
        //AddDWGFilesCommand

        #region Свойства

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

        #region Измерения

        private Point3D? _FirstPoint3D;

        public Point3D? FirstPoint3D
        {
            get
            {
                return _FirstPoint3D;
            }
            set
            {
                Set(ref _FirstPoint3D, value);
            }
        }

        private Point3D? _SecondPoint3D;

        public Point3D? SecondPoint3D
        {
            get
            {
                return _SecondPoint3D;
            }
            set
            {
                Set(ref _SecondPoint3D, value);
            }
        }

        private double _Length1_2;

        public double Length1_2
        {
            get
            {
                return _Length1_2;
            }
            set
            {
                Set(ref _Length1_2, value);
            }
        }

        private double _LengthTotal;

        public double LengthTotal
        {
            get
            {
                return _LengthTotal;
            }
            set
            {
                Set(ref _LengthTotal, value);
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

        //Блокировка окна
        private bool _IsEnableWindow = true;
        public bool IsEnableWindow
        {
            get => _IsEnableWindow;
            set => Set(ref _IsEnableWindow, value);
        }

        //Ортографический вид
        private bool _IsOrthographic = false;
        public bool IsOrthographic
        {
            get => _DrawingControl.Viewport.Orthographic;
            set
            {
                _DrawingControl.Viewport.Orthographic = value;
                Set(ref _IsOrthographic, value);
            }
        }
        

        private bool _IsShowGridLines = true;
        public bool IsShowGridLines
        {
            get 
            { 
                return _DrawingControl.ShowGridLines;
            } 
            set
            {
                _DrawingControl.ShowGridLines = value;
                Set(ref _IsShowGridLines, value);
            }
        }


        #endregion Окно


        #endregion Свойства

        #region Методы

        #region Измерения
        private void DrawingControl_UserModeledDimensionChangedEvent(DrawingControl3D m, Xbim.Presentation.ModelGeomInfo.PolylineGeomInfo e)
        {
            LengthTotal = e.GetLenght();

            SecondPoint3D = FirstPoint3D;

            FirstPoint3D = e.Last3DPoint;

            if (FirstPoint3D != null && SecondPoint3D != null)
            {
                var ResultVector = SecondPoint3D - FirstPoint3D;
                Length1_2 = Math.Sqrt(Math.Pow(ResultVector.Value.X, 2) + Math.Pow(ResultVector.Value.Y, 2) + Math.Pow(ResultVector.Value.Z, 2));
            }
        }

        private void RefreshMeasure()
        {
            LengthTotal = 0;
            Length1_2 = 0;
            FirstPoint3D = null;
            SecondPoint3D = null;
        }

        #endregion Измерения

        #region Загрузка модели
        /// <summary>
        /// Обновление прогресс бара
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ProgressChanged(object sender, ProgressChangedEventArgs args)
        {
            if (sender is null || args.ProgressPercentage < 0 || args.ProgressPercentage > 100)
                return;

            Application.Current.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    ProgressValue = args.ProgressPercentage;
                    StatusMsg = (string)args.UserState;
                }));


        }

        public void ClearViewPortFull(IEnumerable<ModelVisual3D> visual3Ds)
        {
            foreach (ModelVisual3D vs3d in visual3Ds)
            {
                var tt = _DrawingControl.Viewport.Children.FirstOrDefault(it => it.GetHashCode() == vs3d.GetHashCode());
                bool result = _DrawingControl.Viewport.Children.Remove(vs3d);
            }
            _DrawingControl.ReloadModel(DrawingControl3D.ModelRefreshOptions.ViewPreserveCameraPosition);
        }


        public void ClearViewPort(ModelVisual3D visual3D)
        {
            ClearViewPortFull(new List<ModelVisual3D>() { visual3D });
        }

        private void SetVectorOffset(IfcStore ifcStore)
        {
            XbimPoint3D vectorOffsetXbim = _DrawingControl.ModelPositions.GetPoint(new XbimPoint3D(0, 0, 0));

            var xbimRegionCol = ifcStore.Model.GeometryStore.BeginRead().ContextRegions.Where(cr => cr.MostPopulated() != null).Select(c => c.MostPopulated());
            double onMeter = _DrawingControl.ModelPositions._modelPositionsCollection.Values.Select(it => it.OneMeter).First();

            HelixToolkit.Wpf.GridLinesVisual3D? gridLines = _DrawingControl.Viewport.Children.OfType<HelixToolkit.Wpf.GridLinesVisual3D>().FirstOrDefault();
            
            _vectorOffset = new Vector3D();

            if (gridLines != null)
            {
                _vectorOffset.Z = gridLines.Transform.Value.OffsetZ- 3;
            }


            foreach (XbimRegion? xbimRegion in xbimRegionCol)
            {
                double vecOffsetX = xbimRegion.WorldCoordinateSystem.OffsetX / onMeter;
                double vecOffsetY = xbimRegion.WorldCoordinateSystem.OffsetY / onMeter;
                double vecOffsetZ = xbimRegion.WorldCoordinateSystem.OffsetZ / onMeter;

                if (vecOffsetX == 0)
                {
                    _vectorOffset.X = -vectorOffsetXbim.X;
                    _vectorOffset.Y = -vectorOffsetXbim.Y;
                    if (gridLines == null)
                    {
                        _vectorOffset.Z = -vectorOffsetXbim.Z;
                    }
                }
                else
                {
                    _vectorOffset.X = vecOffsetX - vectorOffsetXbim.X;
                    _vectorOffset.Y = vecOffsetY - vectorOffsetXbim.Y;
                    if (gridLines == null)
                    {
                        _vectorOffset.Z = vecOffsetZ - vectorOffsetXbim.Z;
                    }
                }
            }
        }

        private void LoadModel(object sender, DoWorkEventArgs args)
        {

                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    //Закрываем все дочерние окна
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window != mainWindow)
                        {
                            window.Close();
                        }
                    }

                    //Сброс измерений
                    RefreshMeasure();

                    //Очищаем от подложек
                    if (Model != null)
                    {
                        ClearViewPortFull(Model.ModelItems.OfType<ModelItemDWGFile>().Select(it => it.EntityVisual3D));
                    }
                });
       

            try
            {
                string path = args.Argument as string;

                /* Версия с потоком
                FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                IfcStore ifcStore = IfcStore.Open(fileStream, StorageType.Ifc, XbimModelType.MemoryModel, null, XbimDBAccess.Read, _worker.ReportProgress);
                ifcStore.FileName = path;
                */

                //Загружаем ifcStore
                IfcStore ifcStore = IfcStore.Open(path, null, 5000, _worker.ReportProgress, XbimDBAccess.ReadWrite);

                //Запускаем создание геометрии
                Task task = Task.Run(() =>
                {
                    var context = new Xbim3DModelContext(ifcStore.Model);
                    context.CreateContext(_worker.ReportProgress);
                });

                //Создаем модель
                ModelIFC tempModel = ModelIFC.Create(ifcStore,
                                                        _worker,
                                                        ZoomSelected,
                                                        SelectElements,
                                                        HideAfterDelete,
                                                        HideSelected,
                                                        IsolateSelected, 
                                                        ShowSelected,
                                                        RefreshSelect,
                                                        ClearViewPort);

                //Ждем создания геометрии из задачи 
                task.Wait();
                if (tempModel != null)
                {

                    //Разделить на методы
                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        Model?.Dispose();
                        Model = tempModel;
                        if (!ifcStore.GeometryStore.IsEmpty)
                        {
                            mainWindow.WPFDrawingControl.ModelProvider.ObjectInstance = ifcStore;
                            SetVectorOffset(ifcStore);
                        }

                        else
                        {
                            mainWindow.WPFDrawingControl.ModelProvider.ObjectInstance = null;
                        }
                        _deleteEntity = new HashSet<IPersistEntity>();
                        _hideEntity = new HashSet<IPersistEntity>();
                    });
                }
                else
                {
                    throw new FileLoadException("Ошибка загрузки файла");
                }

            }
            catch(FileLoadException fLex)
            {
                MessageBox.Show($"{fLex.Message}", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка {ex}", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                //Application.Current.Dispatcher.BeginInvoke(() => { IsEnableWindow = true; });
                _worker.ReportProgress(0);
                _worker.DoWork -= LoadModel;
            }
        }



        #endregion Загрузка модели

        #region 3D модель

        /// <summary>
        /// Скрываем объекты после удаления
        /// </summary>
        void HideAfterDelete(IEnumerable<IPersistEntity> persistEntitiesAfterDelete)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                _deleteEntity.AddRange(persistEntitiesAfterDelete);
                HideAndUpdate();
            });
        }

        /// <summary>
        /// Скрыть выбранные
        /// </summary>
        private void HideSelected(IEnumerable<IPersistEntity> persistEntitiesForHide)
        {
            _hideEntity.AddRange(persistEntitiesForHide);
            HideAndUpdate();
        }

        /// <summary>
        /// Скрыть объекты и обновить окно
        /// </summary>
        private void HideAndUpdate()
        {
            _hideEntity.AddRange(_deleteEntity);
            _DrawingControl.HiddenInstances = _hideEntity.ToList();

            _DrawingControl.ReloadModel(DrawingControl3D.ModelRefreshOptions.ViewPreserveCameraPosition);
        }

        /// <summary>
        /// Показать выбранные
        /// </summary>
        private void ShowSelected(IEnumerable<IPersistEntity> persistEntitiesForShow)
        {
            foreach (var persistEntity in persistEntitiesForShow)
            {
                _hideEntity.Remove(persistEntity);

                if (_DrawingControl.IsolateInstances != null && _DrawingControl.IsolateInstances.Count >0)
                {
                    if (!_DrawingControl.IsolateInstances.Contains(persistEntity))
                    { 
                        _DrawingControl.IsolateInstances.Add(persistEntity);
                    }
                }
            }
            HideAndUpdate();
        }

        /// <summary>
        /// Изолировать выбранные
        /// </summary>
        private void IsolateSelected(IEnumerable<IPersistEntity> persistEntitiesForIsolate)
        {
            ShowSelected(persistEntitiesForIsolate);
            _DrawingControl.IsolateInstances = persistEntitiesForIsolate.ToList();
            HideAndUpdate();
        }

        public void RefreshSelect()
        {
            _DrawingControl.Selection.Clear();
            _DrawingControl.HighlighSelected(null);
        }

        /// <summary>
        /// Показать все
        /// </summary>
        private void RestoreView()
        {
            _hideEntity = new HashSet<IPersistEntity>();
            _DrawingControl.IsolateInstances = null;
            HideAndUpdate();
        }

        #region Фокус на элемент

        private bool _camChanged;

        private void ZoomSelected(IEnumerable<IPersistEntity> persistEntityForSelect)
        {
            try
            {
                _camChanged = false;
                _DrawingControl.Viewport.Camera.Changed += Camera_Changed;
                //mainWindow.treeViewIFC.SelectedItemChanged -= mainWindow.treeViewIFC_SelectedItemChanged;
                mainWindow.WPFDrawingControl.SelectionChanged -= mainWindow.DrawingControl_SelectedEntityChanged;
   
                _DrawingControl.SelectedEntity = persistEntityForSelect.ToArray()[0];

                if (!_DrawingControl.ZoomSelectedBool())
                {
                    foreach (IPersistEntity persistEntity in persistEntityForSelect)
                    {
                        _DrawingControl.SelectedEntity = persistEntity;

                        if (_DrawingControl.ZoomSelectedBool())
                        {
                            break;
                        }
                    }
                }
                //SelectElements(persistEntityForSelect);
                _DrawingControl.SelectedEntity = persistEntityForSelect.ToArray()[0];

                _DrawingControl.Viewport.Camera.Changed -= Camera_Changed;
                if (!_camChanged)
                    { _DrawingControl.ClipBaseSelected(0.15); }

                //mainWindow.treeViewIFC.SelectedItemChanged += mainWindow.treeViewIFC_SelectedItemChanged;
                mainWindow.WPFDrawingControl.SelectionChanged += mainWindow.DrawingControl_SelectedEntityChanged;
            }
            catch (ArgumentException)
            {

            }

        }

        private void Camera_Changed(object sender, EventArgs e)
        {
            _camChanged = true;
        }

        #endregion Фокус на элемент

        #region Выбрать несколько элементов (не корректно)

        private void SelectElements(IEnumerable<IPersistEntity> persistEntityForSelect)
        {
            _DrawingControl.Selection.Clear();
            
            //_DrawingControl.Selection.AddRange(persistEntityForSelectDistinct);

            HightSelectElement(persistEntityForSelect.Distinct());
        }


        private void HightSelectElement(IEnumerable<IPersistEntity> persistEntityForSelect)
        {

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
  
            Parallel.ForEach(persistEntityForSelect, persistEntity =>
            {
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                    _DrawingControl.Selection.Toggle(persistEntity);
                    _DrawingControl.HighlighSelected(persistEntity);
                });
            });
            cancellationTokenSource.CancelAfter(50);

        }

        #endregion Выбрать элемент





        #endregion 3D модель

        #region Закрыть файл

        public bool CloseApplication()
        {
            if (Model is not null && Model.IsEditModel)
            {
                MessageBoxResult result = MessageBox.Show("Сохранить изменения в модели?", "RZDP IFC Viewer!", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel)
                {
                    return false;
                }
                else if (result == MessageBoxResult.Yes)
                {
                    using (_signal = new ManualResetEvent(false))
                    {
                        OnSaveFileCommandExecuted(this);
                        _signal.WaitOne();
                    }
                }
            }
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

            Model.SaveFile(path, _worker.ReportProgress);

            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                IsEnableWindow = true;
            });
            _worker.DoWork -= SaveModelAsIFC;

            if (_signal is not null)
            {
                _signal.Set();
            }
        }

        private void SaveModelAsIFCXML(object sender, DoWorkEventArgs args)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                IsEnableWindow = false;
            });

            var path = args.Argument as string;

            Model.SaveAsXMLFile(path, _worker.ReportProgress);

            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                IsEnableWindow = true;
            });
            _worker.DoWork -= SaveModelAsIFCXML;
        }

        #endregion Сохранение модели

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
            if (Model is not null && Model.IsEditModel)
            { 
                MessageBoxResult result = MessageBox.Show("В модель были внесены изменения.\n" +
                    "При открытии нового файла изменения не будут сохранены.\n" +
                    "Продолжить?", "RZDP IFC Viewer!", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            string path = o as string;

            if (string.IsNullOrEmpty(path))
            {
                path = HelperFile.OpenIFC_File();
            }

            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            _worker.DoWork += LoadModel;
            _worker.RunWorkerAsync(path);
            //worker.DoWork -= LoadModel;
        }

        private bool CanLoadApplicationCommandExecute(object o)
        {
            return true;
        }

        #endregion Открыть_файл

        #region Обновить

        public ICommand UpdateApplicationCommand { get; }

        private void OnUpdateApplicationCommandExecuted(object o)
        {

            if(!File.Exists(Model.FilePath))
            {
                MessageBox.Show($"Файл\n{Model.FilePath}\nне найден!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
                return ;
            }

            if (Model.IsEditModel)
            {
                MessageBoxResult result = MessageBox.Show("В модель были внесены изменения.\n" +
                    "При обновлении изменения не будут сохранены.\n" +
                    "Продолжить?", "RZDP IFC Viewer!", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }
            _worker.DoWork += LoadModel;
            _worker.RunWorkerAsync(Model.FilePath);
        }

        private bool CanUpdateApplicationCommandExecute(object o)
        {
            return Model is not null;
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
            if (!File.Exists(Model.FilePath))
            {
                MessageBox.Show($"Файл\n{Model.FilePath}\nне найден!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _worker.DoWork += SaveModelAsIFC;
            _worker.RunWorkerAsync(Model.FilePath);
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
            string path = HelperFile.SaveAsIFC_File("ifc");

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            _worker.DoWork += SaveModelAsIFC;
            _worker.RunWorkerAsync(path);
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
            string path = HelperFile.SaveAsIFC_File("ifcxml");

            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            _worker.DoWork += SaveModelAsIFCXML;
            _worker.RunWorkerAsync(path);
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
            string fileHelpPath = AppDomain.CurrentDomain.BaseDirectory + "RZDP IFC Viewer.chm";

            if (System.IO.File.Exists(fileHelpPath))
            {
                Process p = new Process();
                p.StartInfo = new ProcessStartInfo(fileHelpPath)
                {
                    UseShellExecute = true
                };
                p.Start();
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

        #region Показать все

        public ICommand RestoreViewCommand { get; }

        private void OnRestoreViewCommand(object o)
        {
            Model.SetPropertyIsHideToAllElements(false);
            RestoreView();
        }

        private bool CanRestoreViewCommand(object o)
        {
            return Model is not null;
        }

        #endregion Показать все

        #region Скрыть выделенные

        public ICommand HideSelectedModelObjectCommand { get; }

        private void OnHideSelectedModelObjectCommand(object o)
        {
            var hiddenElements = ModelItemIFCObject.SelectionNestedItems(Model.FileItem.ModelProject).
                                                            Where(it => it.IsPaint).
                                                            SelectMany(it => ModelItemIFCObject.SelectionNestedItems(it)).Distinct().ToList();
            Model.SetPropertyIsHideToAllElements(false);
            hiddenElements.ForEach(it => it.IsHidden = true);
            HideSelected(hiddenElements.Select(it => it.GetIFCObjectDefinition()));
        }

        private bool CanHideSelectedModelObjectCommand(object o)
        {
            return Model is not null;
        }

        #endregion Скрыть выделенные

        #region Изолировать выделенные

        public ICommand IsolateSelectedModelObjectCommand { get; }

        private void OnIsolateSelectedModelObjectCommand(object o)
        {
            Model.SetPropertyIsHideToAllElements(true);
            var isolateElements = ModelItemIFCObject.SelectionNestedItems(Model.FileItem.ModelProject).
                                                        Where(it => it.IsPaint).
                                                        SelectMany(it => ModelItemIFCObject.SelectionNestedItems(it)).Distinct().ToList();
            isolateElements.ForEach(it => it.IsHidden = false);
            IsolateSelected(isolateElements.Select(it => it.GetIFCObjectDefinition()));
        }

        private bool CanIsolateSelectedModelObjectCommand(object o)
        {
            return Model is not null;
        }

        #endregion Изолировать выделенные

        #region Добавить файлы dwg

        public ICommand AddDWGFilesCommand { get; }

        private void OnAddDWGFilesCommand(object o)
        {
            try
            {
                string? filePath = HelperFile.OpenDWG_File();
                if (filePath == null)
                    return;

                ModelItemDWGFile modelItemDWGFile = new ModelItemDWGFile(Model, filePath, _vectorOffset);

                Model.ModelItems.Add(modelItemDWGFile);

                _DrawingControl.Viewport.Children.Add(modelItemDWGFile.EntityVisual3D);
                
                _DrawingControl.ReloadModel(DrawingControl3D.ModelRefreshOptions.ViewPreserveCameraPosition);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


        }

        private bool CanAddDWGFilesCommand(object o)
        {
            return Model is not null;
        }

        #endregion Изолировать выделенные

        #endregion Комманды

        public MainWindowViewModel()
        {
        }

        

        public MainWindowViewModel(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            //IsEnableWindow = true;
            _worker = new BackgroundWorker();
            //_DrawingControl.SelectedEntityChanged += DrawingControl_SelectedEntityChanged;
            _DrawingControl.UserModeledDimensionChangedEvent += DrawingControl_UserModeledDimensionChangedEvent;

            _DrawingControl.ViewHome();
            _worker.ProgressChanged += ProgressChanged;
            _worker.WorkerReportsProgress = true;

            _worker.DoWork += Worker_DoWork;
            _worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            var strArray = System.Environment.GetCommandLineArgs();
            if (strArray.Length > 1)
            {
                _worker.DoWork += LoadModel;
                _worker.RunWorkerAsync(Environment.GetCommandLineArgs()[1]);
            }

            #region Комманды

            LoadApplicationCommand = new ActionCommand(
                OnLoadApplicationCommandExecuted,
                CanLoadApplicationCommandExecute);

            UpdateApplicationCommand = new ActionCommand(
                OnUpdateApplicationCommandExecuted,
                CanUpdateApplicationCommandExecute);

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

            RestoreViewCommand = new ActionCommand(
                OnRestoreViewCommand,
                CanRestoreViewCommand);

            HideSelectedModelObjectCommand = new ActionCommand(
                    OnHideSelectedModelObjectCommand,
                    CanHideSelectedModelObjectCommand);

            IsolateSelectedModelObjectCommand = new ActionCommand(
                    OnIsolateSelectedModelObjectCommand,
                    CanIsolateSelectedModelObjectCommand);

            AddDWGFilesCommand = new ActionCommand(
                    OnAddDWGFilesCommand,
                    CanAddDWGFilesCommand);

            #endregion Комманды
        }

    }
}