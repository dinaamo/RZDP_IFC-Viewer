using RZDP_IFC_Viewer.IFC.ModelItem;

namespace RZDP_IFC_Viewer.Infracrucrure.FindObjectException
{
    internal class FindObjectException : Exception
    {
        public ModelItemIFCObject FindObject { get; set; }

        public FindObjectException(ModelItemIFCObject FindObject)
        {
            this.FindObject = FindObject;
        }
    }
}