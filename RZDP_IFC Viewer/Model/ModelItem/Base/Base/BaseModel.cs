using IFC_Table_View.IFC.Model;

namespace RZDP_IFC_Viewer.IFC.Editor.Base
{
    public abstract class BaseModel
    {
        public ModelIFC ModelIFC { get; }

        protected BaseModel(ModelIFC modelIFC)
        {
            this.ModelIFC = modelIFC;
        }
    }
}