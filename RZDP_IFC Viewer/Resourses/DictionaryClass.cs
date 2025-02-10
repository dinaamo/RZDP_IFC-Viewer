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
        }

        public void TreeViewSelectedItemChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TreeViewItem item)
            {
                item.BringIntoView();
                e.Handled = true;
            }
        }

    }
}