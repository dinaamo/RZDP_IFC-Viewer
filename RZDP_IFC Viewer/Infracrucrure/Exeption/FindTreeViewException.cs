using System.Windows.Controls;

namespace RZDP_IFC_Viewer.Infracrucrure.FindObjectException
{
    internal class FindTreeViewException : Exception
    {
        public TreeViewItem searchTreeViewItem { get; set; }

        public FindTreeViewException(TreeViewItem searchTreeViewItem)
        {
            this.searchTreeViewItem = searchTreeViewItem;
        }
    }
}