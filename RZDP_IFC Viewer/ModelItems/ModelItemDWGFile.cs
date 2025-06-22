using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using RZDP_IFC_Viewer.IFC.Model;
using RZDP_IFC_Viewer.Infracrucrure.Commands;
using RZDP_IFC_Viewer.View.Windows;
using RZDP_IFC_Viewer.View.Windows.EditorWindows;
using RZDP_IFC_Viewer.DWG;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using ACadSharp.IO;
using System.Windows.Media.Media3D;

namespace RZDP_IFC_Viewer.IFC.ModelItem
{
    public class ModelItemDWGFile : BaseModelItemIFC
    {
        DWGReader _dWGReader;
        private Vector3D _vectorOffset;
        public ModelVisual3D EntityVisual3D { get; private set; }

        public ModelItemDWGFile(ModelIFC modelIFC, string dwgFilePath, Vector3D vectorOffset) : base(modelIFC)
        {
            _dwgFilePath = dwgFilePath;
            _vectorOffset = vectorOffset;

            _dWGReader = new DWGReader(dwgFilePath, _vectorOffset);

            EntityVisual3D = new ModelVisual3D();

            foreach (Visual3D entity in _dWGReader.ExtractEntityForHelix())
            {
                EntityVisual3D.Children.Add(entity);
            }

            GetPropertyObject();

            DeleteModelDWGFile = new ActionCommand(
               OnDeleteModelDWGFile,
               CanDeleteModelDWGFile);
            _vectorOffset = vectorOffset;

        }


        //public ModelItemDWGFile() :base(null){ }

        #region Свойства

        private string _dwgFilePath;

        public string DWGFileName => Path.GetFileName(_dwgFilePath);

        #endregion

        #region Комманды

        #region Удалить элемент

        public ICommand DeleteModelDWGFile { get; }

        private void OnDeleteModelDWGFile(object o)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
               ModelIFC.ModelItems.Remove(this);
            });
            ModelIFC.СlearViewPort(EntityVisual3D);
        }

        private bool CanDeleteModelDWGFile(object o)
        {
            return true;
        }

        #endregion Удалить элемент

        #endregion

        #region Методы



        #endregion

        public override Dictionary<string, HashSet<object>> PropertyElement
        {
            get
            {
                return GetPropertyObject();
            }
            protected set
            {

            }
        }


        private Dictionary<string, HashSet<object>> GetPropertyObject()
        {
            return new Dictionary<string, HashSet<object>>
            {
                { "Путь к файлу", new HashSet<object>() { _dwgFilePath } }
            };
        }


        //private ObservableCollection<BaseModelItemIFC> _ModelItems;

        //public override ObservableCollection<BaseModelItemIFC> ModelItems
        //{
        //    get
        //    {
        //        if (_ModelItems == null)
        //        {
        //            _ModelItems = new ObservableCollection<BaseModelItemIFC>();
        //        }
        //        return _ModelItems;
        //    }
        //}
    }
}