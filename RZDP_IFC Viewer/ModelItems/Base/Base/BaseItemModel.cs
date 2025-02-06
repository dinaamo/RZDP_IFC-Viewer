using RZDP_IFC_Viewer.IFC.Model;

namespace RZDP_IFC_Viewer.IFC.Editor.Base
{
    public abstract class BaseItemModel
    {
        public ModelIFC ModelIFC { get; }

        protected BaseItemModel(ModelIFC modelIFC)
        {
            this.ModelIFC = modelIFC;
        }
    }
}