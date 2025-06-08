using RZDP_IFC_Viewer.IFC.ModelItem;

namespace RZDP_IFC_Viewer.Infracrucrure.FindObjectException
{
    internal class ExitOperationException : Exception
    {
        public ExitOperationException(string message) : base(message)
        {
                
        }

        public ExitOperationException()
        {
            
        }
    }
}