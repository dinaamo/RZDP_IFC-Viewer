using System.Windows.Controls;

namespace IFC_Table_View.Infracrucrure.FindObjectException
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