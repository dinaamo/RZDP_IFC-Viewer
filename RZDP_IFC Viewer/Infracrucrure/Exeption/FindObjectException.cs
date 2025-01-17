using IFC_Table_View.IFC.ModelItem;

namespace IFC_Table_View.Infracrucrure.FindObjectException
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