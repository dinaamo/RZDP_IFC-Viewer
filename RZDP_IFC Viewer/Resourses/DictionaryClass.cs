using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RZDP_IFC_Viewer.IFC.ModelItem;
using RZDP_IFC_Viewer.View.Windows;

namespace RZDP_IFC_Viewer.Resourses
{
    public partial class DictionaryClass
    {
        public void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                if (sender is TextBlock textBlock)
                {
                    if (textBlock.DataContext is ModelItemIFCTable modelItemIFCTable)
                    {
                        TableWindow.CreateTableWindow(modelItemIFCTable);
                    }
                    else if (textBlock.DataContext is ModelItemDocumentReference documentReference)
                    {
                        documentReference.OpenDocument();
                    }
                }
            }
            //else if(e.ClickCount >= 1)
            //{
            //    if (sender is TextBlock textBlock)
            //    {
            //        if (textBlock.DataContext is ModelItemIFCObject modelItemIFCObject)
            //        {
            //            //modelItemIFCObject.
            //        }
            //    }
            //}
        }

        private void TreeViewSelectedItemChanged(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null)
            {
                item.BringIntoView();
                e.Handled = true;

                if (item.DataContext is ModelItemIFCObject modelItemIFCObject)
                {
                    modelItemIFCObject.SelectElements();
                }
            }
        }

        //private void MouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.Source is TreeViewItem treeViewItem)
        //    {
        //        if (treeViewItem.DataContext is ModelItemIFCObject modelItemObject)
        //        {
        //        }
        //    }
        //}
    }
}